using System;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Dtos;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Interfaces;

namespace HotelApp.BL.Services
{
    public class ReportService : IReportService
    {
        // Statuses that generate revenue (cancelled and no-show do not; their penalties are counted separately)
        private static readonly string[] BillableStatuses =
        {
            ReservationStatus.Confirmed, ReservationStatus.InProgress, ReservationStatus.Completed
        };

        private readonly IReservationRepository _reservations;
        private readonly IReservationDetailRepository _details;
        private readonly IRoomRepository _rooms;
        private readonly IGuestRepository _guests;
        private readonly IReservationAuditRepository _audit;
        private readonly IUserRepository _users;

        public ReportService(
            IReservationRepository reservations,
            IReservationDetailRepository details,
            IRoomRepository rooms,
            IGuestRepository guests,
            IReservationAuditRepository audit,
            IUserRepository users)
        {
            _reservations = reservations;
            _details = details;
            _rooms = rooms;
            _guests = guests;
            _audit = audit;
            _users = users;
        }

        // RF-015
        public async Task<ReservationsReportDto> ReservationsByRangeAsync(DateTime from, DateTime to, int requestingUserId)
        {
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);
            var (start, endExclusive) = ValidateRange(from, to);

            var reservations = (await _reservations.GetByDateRangeAsync(start, endExclusive)).ToList();
            var guests = (await _guests.GetAllAsync()).ToDictionary(g => g.GuestId);
            var rooms = (await _rooms.GetAllAsync()).ToDictionary(r => r.RoomId);
            var details = (await _details.GetByReservationsAsync(reservations.Select(r => r.ReservationId))).ToLookup(d => d.ReservationId);

            var rows = reservations.Select(r =>
            {
                var guest = guests[r.GuestId];
                var roomNumbers = details[r.ReservationId].Select(d => rooms[d.RoomId].Number).OrderBy(n => n);

                return new ReservationSummaryDto
                {
                    ReservationId = r.ReservationId,
                    ReservationCode = r.ReservationCode,
                    Guest = $"{guest.FirstName} {guest.LastName}",
                    Rooms = string.Join(", ", roomNumbers),
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Status = r.Status,
                    Total = r.Total
                };
            }).ToList();

            return new ReservationsReportDto
            {
                From = start,
                To = to.Date,
                Reservations = rows,
                TotalReservations = rows.Count,
                ReservationsByStatus = rows.GroupBy(row => row.Status).ToDictionary(g => g.Key, g => g.Count()),
                EstimatedRevenue = reservations.Where(r => BillableStatuses.Contains(r.Status)).Sum(r => r.Total),
                PenaltyRevenue = await SumPenaltiesAsync(start, endExclusive)
            };
        }

        // CU-014
        public async Task<OccupancyReportDto> OccupancyAsync(DateTime from, DateTime to, int requestingUserId)
        {
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);
            var (start, endExclusive) = ValidateRange(from, to);

            var daysInRange = (endExclusive - start).Days;
            var totalRooms = (await _rooms.GetAllAsync()).Count(r => r.IsActive);

            var reservations = (await _reservations.GetByDateRangeAsync(start, endExclusive))
                .Where(r => BillableStatuses.Contains(r.Status))
                .ToDictionary(r => r.ReservationId);

            var details = await _details.GetByReservationsAsync(reservations.Keys);

            var occupiedNights = 0;
            decimal revenue = 0;

            foreach (var detail in details)
            {
                var reservation = reservations[detail.ReservationId];

                // Only the nights of the reservation that fall inside the requested range count
                var stayStart = reservation.CheckInDate.Date > start ? reservation.CheckInDate.Date : start;
                var stayEnd = reservation.CheckOutDate.Date < endExclusive ? reservation.CheckOutDate.Date : endExclusive;
                var nights = Math.Max(0, (stayEnd - stayStart).Days);

                occupiedNights += nights;
                revenue += nights * detail.UnitPrice;
            }

            var availableNights = totalRooms * daysInRange;

            return new OccupancyReportDto
            {
                From = start,
                To = to.Date,
                TotalRooms = totalRooms,
                AvailableNights = availableNights,
                OccupiedNights = occupiedNights,
                OccupancyPercentage = availableNights == 0 ? 0 : Math.Round(occupiedNights * 100m / availableNights, 2),
                LodgingRevenue = revenue,
                PenaltyRevenue = await SumPenaltiesAsync(start, endExclusive)
            };
        }

        private async Task<decimal> SumPenaltiesAsync(DateTime start, DateTime endExclusive)
        {
            var cancellations = await _audit.GetByRangeAsync(start, endExclusive, AuditAction.Cancelled);
            return cancellations.Sum(a => a.Amount ?? 0m);
        }

        private static (DateTime start, DateTime endExclusive) ValidateRange(DateTime from, DateTime to)
        {
            if (to.Date < from.Date)
                throw new BusinessRuleException(ErrorCodes.Validation, "La fecha final del reporte no puede ser anterior a la inicial.");

            return (from.Date, to.Date.AddDays(1));
        }
    }
}
