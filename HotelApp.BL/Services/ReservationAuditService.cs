using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Dtos;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Services
{
    public class ReservationAuditService : IReservationAuditService
    {
        private const string SystemUserName = "Sistema";

        private readonly IReservationAuditRepository _repository;
        private readonly IReservationRepository _reservations;
        private readonly IUserRepository _users;

        public ReservationAuditService(
            IReservationAuditRepository repository,
            IReservationRepository reservations,
            IUserRepository users)
        {
            _repository = repository;
            _reservations = reservations;
            _users = users;
        }

        // RNF-007
        public async Task<IEnumerable<ReservationAuditDto>> GetByReservationAsync(int reservationId, int requestingUserId)
        {
            Validations.ValidId(reservationId, "la reserva");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var reservation = await _reservations.GetByIdAsync(reservationId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "La reserva no existe.");

            var records = await _repository.GetByReservationAsync(reservationId);
            return await ToDtosAsync(records, new Dictionary<int, string> { [reservation.ReservationId] = reservation.ReservationCode });
        }

        // RNF-007
        public async Task<IEnumerable<ReservationAuditDto>> GetByRangeAsync(DateTime from, DateTime to, string? action, int requestingUserId)
        {
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            if (to.Date < from.Date)
                throw new BusinessRuleException(ErrorCodes.Validation, "La fecha final no puede ser anterior a la inicial.");

            string? cleanAction = null;
            if (!string.IsNullOrWhiteSpace(action))
            {
                var allActions = new[]
                {
                    AuditAction.Created, AuditAction.Confirmed, AuditAction.Modified, AuditAction.Cancelled,
                    AuditAction.CheckIn, AuditAction.CheckOut, AuditAction.NoShow
                };

                cleanAction = allActions.FirstOrDefault(a => string.Equals(a, action.Trim(), StringComparison.OrdinalIgnoreCase))
                    ?? throw new BusinessRuleException(ErrorCodes.Validation,
                        $"La acción '{action}' no es válida. Acciones permitidas: {string.Join(", ", allActions)}.");
            }

            var records = (await _repository.GetByRangeAsync(from.Date, to.Date.AddDays(1), cleanAction)).ToList();

            var reservationIds = records.Select(r => r.ReservationId).ToHashSet();
            var codes = (await _reservations.GetAllAsync())
                .Where(r => reservationIds.Contains(r.ReservationId))
                .ToDictionary(r => r.ReservationId, r => r.ReservationCode);

            return await ToDtosAsync(records, codes);
        }

        private async Task<List<ReservationAuditDto>> ToDtosAsync(IEnumerable<ReservationAudit> records, IReadOnlyDictionary<int, string> reservationCodes)
        {
            var userNames = (await _users.GetAllAsync()).ToDictionary(u => u.UserId, u => u.Name);

            return records.Select(a => new ReservationAuditDto
            {
                ReservationAuditId = a.ReservationAuditId,
                ReservationId = a.ReservationId,
                ReservationCode = reservationCodes.TryGetValue(a.ReservationId, out var code) ? code : string.Empty,
                UserName = a.UserId is int userId && userNames.TryGetValue(userId, out var name) ? name : SystemUserName,
                Action = a.Action,
                Timestamp = a.Timestamp,
                Amount = a.Amount,
                Details = a.Details
            }).ToList();
        }
    }
}
