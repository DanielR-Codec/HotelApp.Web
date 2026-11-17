using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Dtos;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;
using HotelApp.DAL.Constants;

namespace HotelApp.BL.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repository;
        private readonly IRoomTypeRepository _roomTypes;
        private readonly IFloorRepository _floors;
        private readonly IBlockRepository _blocks;
        private readonly IHotelRepository _hotels;
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public RoomService(
            IRoomRepository repository,
            IRoomTypeRepository roomTypes,
            IFloorRepository floors,
            IBlockRepository blocks,
            IHotelRepository hotels,
            IUserRepository users,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _repository = repository;
            _roomTypes = roomTypes;
            _floors = floors;
            _blocks = blocks;
            _hotels = hotels;
            _users = users;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<IEnumerable<Room>> GetAllAsync(int? hotelId = null, int? blockId = null, int? floorId = null, bool includeInactive = false)
        {
            return await _repository.GetFilteredAsync(hotelId, blockId, floorId, includeInactive);
        }

        public async Task<Room?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        // CU-004 / RF-004 / RN-001 / RN-003 / RN-005 / RN-011
        public async Task<IEnumerable<AvailableRoomDto>> SearchAvailabilityAsync(DateTime checkInDate, DateTime checkOutDate, int guestCount, int? hotelId = null)
        {
            var now = _timeProvider.GetLocalNow().DateTime;
            var checkIn = checkInDate.Date;
            var checkOut = checkOutDate.Date;

            Validations.ValidateStayDates(checkIn, checkOut, now);   // alternate flow 2a

            if (guestCount < 1)
                throw new BusinessRuleException(ErrorCodes.Validation, "La cantidad de personas debe ser al menos 1.");

            var rooms = await _repository.SearchAvailableAsync(checkIn, checkOut, guestCount, hotelId);
            var roomTypes = (await _roomTypes.GetAllAsync()).ToDictionary(t => t.RoomTypeId);
            var floors = (await _floors.GetAllAsync()).ToDictionary(f => f.FloorId);
            var blocks = (await _blocks.GetAllAsync()).ToDictionary(b => b.BlockId);
            var hotels = (await _hotels.GetAllAsync()).ToDictionary(h => h.HotelId);
            var nights = (checkOut - checkIn).Days;

            // If the list is empty, the screen shows "no availability" (alternate flow 4a)
            return rooms.Select(r =>
            {
                var roomType = roomTypes[r.RoomTypeId];
                var floor = floors[r.FloorId];
                var block = blocks[floor.BlockId];
                var hotel = hotels[block.HotelId];

                return new AvailableRoomDto
                {
                    RoomId = r.RoomId,
                    Number = r.Number,
                    HotelId = hotel.HotelId,
                    HotelName = hotel.Name,
                    BlockName = block.Name,
                    FloorNumber = floor.Number,
                    RoomTypeId = roomType.RoomTypeId,
                    RoomTypeName = roomType.Name,
                    MaxCapacity = roomType.MaxCapacity,
                    RatePerNight = roomType.BaseRate,
                    Nights = nights,
                    EstimatedTotal = roomType.BaseRate * nights
                };
            }).ToList();
        }

        // CU-011 / RF-001
        public async Task AddAsync(Room room, int requestingUserId)
        {
            Validations.NotNull(room, "la habitación");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);
            await ValidateAsync(room, null);

            room.CurrentStatus = RoomStatus.Available;
            room.IsActive = true;

            await _repository.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();
        }

        // CU-011 / RF-001 / RF-002. The status is not changed here: use ChangeStatusAsync for that.
        public async Task UpdateAsync(Room room, int requestingUserId)
        {
            Validations.NotNull(room, "la habitación");
            Validations.ValidId(room.RoomId, "la habitación");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var existing = await _repository.GetByIdAsync(room.RoomId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "La habitación no existe.");

            await ValidateAsync(room, existing.RoomId);

            // Taking a room out of service follows the same rule as DeleteAsync
            if (existing.IsActive && !room.IsActive)
                await EnsureCanDeactivateAsync(existing);

            existing.Number = room.Number;
            existing.FloorId = room.FloorId;
            existing.RoomTypeId = room.RoomTypeId;
            existing.IsActive = room.IsActive;

            await _unitOfWork.SaveChangesAsync();
        }

        // RF-001: soft delete
        public async Task DeleteAsync(int roomId, int requestingUserId)
        {
            Validations.ValidId(roomId, "la habitación");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var room = await _repository.GetByIdAsync(roomId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "La habitación no existe.");

            if (!room.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, $"La habitación {room.Number} ya está dada de baja.");

            await EnsureCanDeactivateAsync(room);

            room.IsActive = false;
            await _unitOfWork.SaveChangesAsync();
        }

        // CU-012 / RF-010
        public async Task ChangeStatusAsync(int roomId, string newStatus, int userId)
        {
            Validations.ValidId(roomId, "la habitación");
            Validations.Required(newStatus, "Estado");
            await Permissions.GetActiveUserAsync(_users, userId);

            var room = await _repository.GetByIdAsync(roomId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "La habitación no existe.");

            if (!room.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, "La habitación está dada de baja.");

            var allowed = new[] { RoomStatus.Available, RoomStatus.Cleaning, RoomStatus.Maintenance };
            var status = allowed.FirstOrDefault(s => string.Equals(s, newStatus?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (status is null)
                throw new BusinessRuleException(ErrorCodes.Validation,
                    $"Estado no permitido. Manualmente solo se puede cambiar a: {string.Join(", ", allowed)}. '{RoomStatus.Occupied}' se asigna con el Check-in.");

            if (room.CurrentStatus == RoomStatus.Occupied)
                throw new BusinessRuleException(ErrorCodes.Validation, $"La habitación {room.Number} tiene un huésped dentro. Primero debe hacerse el Check-out.");

            room.CurrentStatus = status;
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureCanDeactivateAsync(Room room)
        {
            var today = _timeProvider.GetLocalNow().DateTime.Date;
            if (await _repository.HasUpcomingReservationsAsync(room.RoomId, today))
                throw new BusinessRuleException(ErrorCodes.Validation, $"No se puede dar de baja la habitación {room.Number}: tiene reservas vigentes.");
        }

        private async Task ValidateAsync(Room room, int? existingId)
        {
            room.Number = Validations.MaxLength(Validations.Required(room.Number, "Número"), 10, "Número");

            // The room must be on an existing, active floor
            Validations.ValidId(room.FloorId, "el piso");
            var floor = await _floors.GetByIdAsync(room.FloorId)
                ?? throw new BusinessRuleException(ErrorCodes.Validation, "El piso seleccionado no existe.");

            if (!floor.IsActive && room.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, "El piso seleccionado está inactivo.");

            // RF-002: every room has a type (and with it, its base rate)
            if (await _roomTypes.GetByIdAsync(room.RoomTypeId) is null)
                throw new BusinessRuleException(ErrorCodes.Validation, "El tipo de habitación seleccionado no existe.");

            // The room number cannot repeat inside the same hotel
            var hotelId = await _floors.GetHotelIdAsync(room.FloorId) ?? 0;
            if (await _repository.NumberExistsInHotelAsync(hotelId, room.Number, existingId))
                throw new BusinessRuleException(ErrorCodes.Validation, $"Ya existe una habitación con el número '{room.Number}' en este hotel.");
        }
    }
}
