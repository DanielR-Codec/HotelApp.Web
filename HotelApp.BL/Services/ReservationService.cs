using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Dtos;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;
using HotelApp.DAL.Constants;
using Microsoft.Extensions.Logging;

namespace HotelApp.BL.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repository;
        private readonly IReservationDetailRepository _details;
        private readonly IRoomRepository _rooms;
        private readonly IRoomTypeRepository _roomTypes;
        private readonly IGuestRepository _guests;
        private readonly IUserRepository _users;
        private readonly IReservationAuditRepository _audit;
        private readonly IEmailService _email;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository repository,
            IReservationDetailRepository details,
            IRoomRepository rooms,
            IRoomTypeRepository roomTypes,
            IGuestRepository guests,
            IUserRepository users,
            IReservationAuditRepository audit,
            IEmailService email,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider,
            ILogger<ReservationService> logger)
        {
            _repository = repository;
            _details = details;
            _rooms = rooms;
            _roomTypes = roomTypes;
            _guests = guests;
            _users = users;
            _audit = audit;
            _email = email;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        private DateTime Now => _timeProvider.GetLocalNow().DateTime;

        // ------------------------------------------------------------------ queries

        public async Task<IEnumerable<Reservation>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Reservation?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<FullReservationDto?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var reservation = await _repository.GetByCodeAsync(code.Trim().ToUpperInvariant());
            return reservation is null ? null : await BuildDtoAsync(reservation);
        }

        public async Task<IEnumerable<Reservation>> GetByGuestAsync(int guestId) => await _repository.GetByGuestAsync(guestId);

        public async Task<IEnumerable<Reservation>> GetArrivalsForDateAsync(DateTime? date = null)
        {
            return await _repository.GetArrivalsForDateAsync((date ?? Now).Date);
        }

        // ------------------------------------------------------------------ CU-005 Create reservation

        public async Task<Reservation> CreateAsync(CreateReservationRequest request)
        {
            Validations.NotNull(request, "la reserva");
            Validations.ValidId(request.GuestId, "el huésped");
            Validations.ValidId(request.UserId, "el usuario");

            var now = Now;
            var checkIn = request.CheckInDate.Date;
            var checkOut = request.CheckOutDate.Date;

            // ---- Validations that do not need the database
            Validations.ValidateStayDates(checkIn, checkOut, now);                          // RN-003

            if (request.GuestCount < 1)
                throw new BusinessRuleException(ErrorCodes.Validation, "La cantidad de personas debe ser al menos 1.");

            var roomIds = (request.RoomIds ?? new List<int>()).Distinct().ToList();
            if (roomIds.Count == 0)
                throw new BusinessRuleException(ErrorCodes.Validation, "Debe seleccionar al menos una habitación.");

            if (request.DiscountPercentage < 0 || request.DiscountPercentage > 100)
                throw new BusinessRuleException(ErrorCodes.Validation, "El descuento debe estar entre 0 y 100%.");

            var user = await Permissions.GetActiveUserAsync(_users, request.UserId);

            // RN-012: reception cannot give more than half off
            if (request.DiscountPercentage > HotelRules.MaxReceptionistDiscount && user.Role != UserRole.Administrator)
                throw new BusinessRuleException("RN-012", $"Un recepcionista no puede aplicar un descuento mayor al {HotelRules.MaxReceptionistDiscount:0}% de la tarifa.");

            var guest = await _guests.GetByIdAsync(request.GuestId)
                ?? throw new BusinessRuleException(ErrorCodes.Validation, "El huésped titular no existe. Regístrelo primero (CU-002).");

            // RN-002
            if (!Validations.IsAdult(guest.BirthDate, now))
                throw new BusinessRuleException("RN-002", "El huésped titular debe ser mayor de edad.");

            // ---- Everything else in a SERIALIZABLE transaction (RNF-006 and the analysis conclusion):
            // if two receptionists book the same room at the same time, SQL Server does not let
            // both pass the availability check; one of them fails and is rolled back.
            var reservation = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var rooms = (await _rooms.GetByIdsAsync(roomIds)).ToList();

                if (rooms.Count != roomIds.Count || rooms.Any(r => !r.IsActive))
                    throw new BusinessRuleException(ErrorCodes.Validation, "Alguna de las habitaciones seleccionadas no existe o está dada de baja.");

                // RN-011
                var underMaintenance = rooms.FirstOrDefault(r => r.CurrentStatus == RoomStatus.Maintenance);
                if (underMaintenance is not null)
                    throw new BusinessRuleException("RN-011", $"La habitación {underMaintenance.Number} está en Mantenimiento y no se puede reservar.");

                // RN-013
                var duplicate = await _repository.GetDuplicateReservationAsync(request.GuestId, checkIn, checkOut, roomIds);
                if (duplicate is not null)
                    throw new BusinessRuleException("RN-013", $"Este huésped ya tiene la reserva {duplicate.ReservationCode} para esas habitaciones en fechas que se solapan.");

                // RN-001 (alternate flow 4a: "someone else just took it")
                foreach (var room in rooms)
                {
                    if (!await _rooms.IsAvailableAsync(room.RoomId, checkIn, checkOut))
                        throw new BusinessRuleException("RN-001", $"La habitación {room.Number} ya no está disponible para esas fechas.");
                }

                // RN-005: the sum of capacities must fit every guest
                var roomTypes = (await _roomTypes.GetAllAsync()).ToDictionary(t => t.RoomTypeId);
                var totalCapacity = rooms.Sum(r => roomTypes[r.RoomTypeId].MaxCapacity);
                if (request.GuestCount > totalCapacity)
                    throw new BusinessRuleException("RN-005", $"Las habitaciones elegidas admiten {totalCapacity} persona(s) y se pidieron {request.GuestCount}.");

                // Reservation (saved first to get its ReservationId)
                var newReservation = new Reservation
                {
                    ReservationCode = await GenerateCodeAsync(now),
                    GuestId = request.GuestId,
                    UserId = request.UserId,
                    CheckInDate = checkIn,
                    CheckOutDate = checkOut,
                    Status = request.GuaranteePaymentReceived ? ReservationStatus.Confirmed : ReservationStatus.Pending,   // RN-004
                    Total = 0,
                    CreatedAt = now
                };

                await _repository.AddAsync(newReservation);
                await _unitOfWork.SaveChangesAsync();

                // ReservationDetail + RF-006: the total is computed from nights × rates, never by hand
                var nights = (checkOut - checkIn).Days;
                var factor = 1 - request.DiscountPercentage / 100m;
                decimal total = 0;

                foreach (var room in rooms)
                {
                    var price = Math.Round(roomTypes[room.RoomTypeId].BaseRate * factor, 2);

                    await _details.AddAsync(new ReservationDetail
                    {
                        ReservationId = newReservation.ReservationId,
                        RoomId = room.RoomId,
                        UnitPrice = price
                    });

                    total += price * nights;
                }

                newReservation.Total = total;

                await _audit.AddAsync(CreateAudit(newReservation.ReservationId, request.UserId, AuditAction.Created, now,
                    details: $"Estado inicial: {newReservation.Status}. Habitaciones: {string.Join(", ", rooms.Select(r => r.Number))}."));

                return newReservation;
            }, IsolationLevel.Serializable);

            // RF-013 (CU-015): after the commit, so an email failure never affects the reservation
            await TryNotifyAsync(async () =>
            {
                var dto = await BuildDtoAsync(reservation);
                await _email.SendReservationConfirmationAsync(dto);
            });

            return reservation;
        }

        // ------------------------------------------------------------------ RN-004

        public async Task ConfirmAsync(int reservationId, int userId)
        {
            var now = Now;
            await Permissions.GetActiveUserAsync(_users, userId);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var reservation = await GetReservationOrThrowAsync(reservationId);

                if (reservation.Status != ReservationStatus.Pending)
                    throw new BusinessRuleException(ErrorCodes.Validation, $"Solo se puede confirmar una reserva Pendiente (estado actual: {reservation.Status}).");

                reservation.Status = ReservationStatus.Confirmed;
                await _audit.AddAsync(CreateAudit(reservation.ReservationId, userId, AuditAction.Confirmed, now, details: "Pago de garantía recibido."));
            });
        }

        // ------------------------------------------------------------------ CU-006 Edit reservation

        public async Task UpdateDatesAsync(int reservationId, DateTime newCheckIn, DateTime newCheckOut, int userId)
        {
            var now = Now;
            var checkIn = newCheckIn.Date;
            var checkOut = newCheckOut.Date;

            Validations.ValidateStayDates(checkIn, checkOut, now);                          // RN-003
            await Permissions.GetActiveUserAsync(_users, userId);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var reservation = await GetReservationOrThrowAsync(reservationId);

                if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Confirmed)
                    throw new BusinessRuleException(ErrorCodes.Validation, $"Solo se pueden mover las fechas de reservas Pendientes o Confirmadas (estado actual: {reservation.Status}).");

                var details = (await _details.GetByReservationAsync(reservationId)).ToList();
                var rooms = (await _rooms.GetByIdsAsync(details.Select(d => d.RoomId))).ToList();

                // RN-015: dates cannot be moved blindly; every room is validated again
                // (ignoring this same reservation, which already holds the old dates)
                foreach (var room in rooms)
                {
                    if (room.CurrentStatus == RoomStatus.Maintenance)
                        throw new BusinessRuleException("RN-011", $"La habitación {room.Number} está en Mantenimiento.");

                    if (!await _rooms.IsAvailableAsync(room.RoomId, checkIn, checkOut, reservationId))
                        throw new BusinessRuleException("RN-015", $"La habitación {room.Number} no está disponible en las nuevas fechas.");
                }

                var previous = $"{reservation.CheckInDate:dd/MM/yyyy} - {reservation.CheckOutDate:dd/MM/yyyy}";

                reservation.CheckInDate = checkIn;
                reservation.CheckOutDate = checkOut;
                // Prices agreed at booking time are kept; only the number of nights changes
                reservation.Total = details.Sum(d => d.UnitPrice) * (checkOut - checkIn).Days;

                await _audit.AddAsync(CreateAudit(reservation.ReservationId, userId, AuditAction.Modified, now,
                    details: $"Fechas: {previous} → {checkIn:dd/MM/yyyy} - {checkOut:dd/MM/yyyy}."));
            }, IsolationLevel.Serializable);
        }

        // ------------------------------------------------------------------ CU-007 Cancel reservation

        public async Task<CancellationResult> CancelAsync(int reservationId, int userId)
        {
            var now = Now;
            await Permissions.GetActiveUserAsync(_users, userId);

            var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var reservation = await GetReservationOrThrowAsync(reservationId);

                // Alternate flow 3a
                if (reservation.Status == ReservationStatus.InProgress)
                    throw new BusinessRuleException("CU-007", "El huésped ya hizo Check-in: no se puede cancelar, hay que hacerle el Check-out normal.");

                if (reservation.Status != ReservationStatus.Pending && reservation.Status != ReservationStatus.Confirmed)
                    throw new BusinessRuleException(ErrorCodes.Validation, $"La reserva está '{reservation.Status}' y no se puede cancelar.");

                var details = (await _details.GetByReservationAsync(reservationId)).ToList();

                // RN-008: cancelling on the arrival day (or later) charges 100% of the first night
                decimal penalty = 0;
                if (now.Date >= reservation.CheckInDate.Date)
                    penalty = details.Sum(d => d.UnitPrice);

                // RF-007: once Cancelled the reservation no longer blocks the rooms (availability only looks at active reservations)
                reservation.Status = ReservationStatus.Cancelled;

                // RNF-007: who, when and how much
                await _audit.AddAsync(CreateAudit(reservation.ReservationId, userId, AuditAction.Cancelled, now, penalty,
                    penalty > 0 ? "Cancelación tardía: se cobra la primera noche." : "Cancelación sin cargo."));

                return new CancellationResult
                {
                    ReservationId = reservation.ReservationId,
                    ReservationCode = reservation.ReservationCode,
                    PenaltyApplied = penalty,
                    Message = penalty > 0
                        ? $"Reserva cancelada. Se aplica una multa de RD$ {penalty:N2} (100% de la primera noche)."
                        : "Reserva cancelada sin cargo."
                };
            });

            // RF-014 (CU-015)
            await TryNotifyAsync(async () =>
            {
                var reservation = await GetReservationOrThrowAsync(reservationId);
                var dto = await BuildDtoAsync(reservation);
                await _email.SendReservationCancellationAsync(dto, result.PenaltyApplied);
            });

            return result;
        }

        // ------------------------------------------------------------------ CU-008 Check-in

        public async Task CheckInAsync(int reservationId, int userId)
        {
            var now = Now;
            await Permissions.GetActiveUserAsync(_users, userId);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var reservation = await GetReservationOrThrowAsync(reservationId);

                if (reservation.Status == ReservationStatus.Pending)
                    throw new BusinessRuleException("RN-004", "La reserva está Pendiente: falta confirmar el pago de garantía antes del Check-in.");

                if (reservation.Status != ReservationStatus.Confirmed)
                    throw new BusinessRuleException(ErrorCodes.Validation, $"Solo se puede hacer Check-in a reservas Confirmadas (estado actual: {reservation.Status}).");

                // Precondition: today is the arrival date
                if (reservation.CheckInDate.Date != now.Date)
                    throw new BusinessRuleException(ErrorCodes.Validation, $"Hoy no es la fecha de llegada de esta reserva (llegada: {reservation.CheckInDate:dd/MM/yyyy}).");

                // RN-006
                if (now.TimeOfDay < HotelRules.CheckInTime)
                    throw new BusinessRuleException("RN-006", $"El Check-in oficial es a partir de las {HotelRules.CheckInTime:hh\\:mm}.");

                var details = (await _details.GetByReservationAsync(reservationId)).ToList();
                var rooms = (await _rooms.GetByIdsAsync(details.Select(d => d.RoomId))).ToList();

                // Alternate flow 3a: if the room is not ready yet, ask the guest to wait
                foreach (var room in rooms)
                {
                    if (room.CurrentStatus != RoomStatus.Available)
                        throw new BusinessRuleException("CU-008",
                            $"La habitación {room.Number} todavía está '{room.CurrentStatus}'. Pídale al huésped que espere.");
                }

                foreach (var room in rooms)
                    room.CurrentStatus = RoomStatus.Occupied;

                reservation.Status = ReservationStatus.InProgress;

                await _audit.AddAsync(CreateAudit(reservation.ReservationId, userId, AuditAction.CheckIn, now));
            });
        }

        // ------------------------------------------------------------------ CU-009 Check-out

        public async Task<CheckOutResult> CheckOutAsync(int reservationId, int userId)
        {
            var now = Now;
            await Permissions.GetActiveUserAsync(_users, userId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var reservation = await GetReservationOrThrowAsync(reservationId);

                if (reservation.Status != ReservationStatus.InProgress)
                    throw new BusinessRuleException(ErrorCodes.Validation, $"Solo se puede hacer Check-out a reservas En Curso (estado actual: {reservation.Status}).");

                var details = (await _details.GetByReservationAsync(reservationId)).ToList();
                var rooms = (await _rooms.GetByIdsAsync(details.Select(d => d.RoomId))).ToList();

                // RN-010: when the guest leaves, each room automatically goes to "Cleaning"
                foreach (var room in rooms)
                    room.CurrentStatus = RoomStatus.Cleaning;

                reservation.Status = ReservationStatus.Completed;

                await _audit.AddAsync(CreateAudit(reservation.ReservationId, userId, AuditAction.CheckOut, now));

                // RN-007: the room must be handed back by 12:00 on the check-out day
                var limit = reservation.CheckOutDate.Date + HotelRules.CheckOutTime;
                var isLate = now > limit;

                return new CheckOutResult
                {
                    ReservationCode = reservation.ReservationCode,
                    LateCheckOut = isLate,
                    Message = isLate
                        ? $"Check-out registrado, pero fuera de hora: la habitación debía entregarse a las {HotelRules.CheckOutTime:hh\\:mm}."
                        : $"Check-out registrado. Las habitaciones pasaron a '{RoomStatus.Cleaning}'."
                };
            });
        }

        // ------------------------------------------------------------------ RN-009 No-Show

        public async Task<int> ProcessNoShowsAsync()
        {
            var now = Now;

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var candidates = await _repository.GetWithoutCheckInUntilAsync(now.Date);

                // Arrivals from previous days, or from today once 11:59 PM has passed
                var expired = candidates
                    .Where(r => r.CheckInDate.Date < now.Date || now.TimeOfDay >= HotelRules.NoShowCutoffTime)
                    .ToList();

                foreach (var reservation in expired)
                {
                    reservation.Status = ReservationStatus.NoShow;   // frees the rooms so they can be sold
                    await _audit.AddAsync(CreateAudit(reservation.ReservationId, null, AuditAction.NoShow, now,
                        details: "El huésped no se presentó; reserva dada de baja automáticamente."));
                }

                return expired.Count;
            });
        }

        // ------------------------------------------------------------------ private

        private async Task<Reservation> GetReservationOrThrowAsync(int reservationId)
        {
            return await _repository.GetByIdAsync(reservationId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "La reserva no existe.");
        }

        private static ReservationAudit CreateAudit(int reservationId, int? userId, string action, DateTime timestamp, decimal? amount = null, string? details = null)
        {
            return new ReservationAudit
            {
                ReservationId = reservationId,
                UserId = userId,
                Action = action,
                Timestamp = timestamp,
                Amount = amount,
                Details = details
            };
        }

        /// <summary>Code like RES-20261110-K7M2P. It is checked to be unique (and there is also a unique index in the database).</summary>
        private async Task<string> GenerateCodeAsync(DateTime date)
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";   // no confusing letters/digits (0, O, 1, I)

            for (var attempt = 0; attempt < 10; attempt++)
            {
                var suffix = new string(Enumerable.Range(0, 5)
                    .Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)])
                    .ToArray());

                var code = $"RES-{date:yyyyMMdd}-{suffix}";

                if (!await _repository.CodeExistsAsync(code))
                    return code;
            }

            throw new InvalidOperationException("Could not generate a unique reservation code.");
        }

        private async Task<FullReservationDto> BuildDtoAsync(Reservation reservation)
        {
            var guest = await _guests.GetByIdAsync(reservation.GuestId);
            var details = (await _details.GetByReservationAsync(reservation.ReservationId)).ToList();
            var rooms = (await _rooms.GetByIdsAsync(details.Select(d => d.RoomId)))
                .ToDictionary(r => r.RoomId);
            var roomTypes = (await _roomTypes.GetAllAsync()).ToDictionary(t => t.RoomTypeId);

            var nights = (reservation.CheckOutDate.Date - reservation.CheckInDate.Date).Days;

            return new FullReservationDto
            {
                Reservation = reservation,
                Guest = guest,
                Nights = nights,
                Rooms = details.Select(d =>
                {
                    var room = rooms[d.RoomId];
                    return new ReservationDetailDto
                    {
                        ReservationDetailId = d.ReservationDetailId,
                        RoomId = room.RoomId,
                        RoomNumber = room.Number,
                        RoomType = roomTypes[room.RoomTypeId].Name,
                        PricePerNight = d.UnitPrice,
                        Subtotal = d.UnitPrice * nights
                    };
                }).ToList()
            };
        }

        /// <summary>Emails are sent once the operation is already saved: if something fails here, it is only logged.</summary>
        private async Task TryNotifyAsync(Func<Task> send)
        {
            try
            {
                await send();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The operation was saved, but the email notification could not be sent.");
            }
        }
    }
}
