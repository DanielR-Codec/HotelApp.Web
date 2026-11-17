using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;

namespace HotelApp.BL.Services
{
    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _repository;
        private readonly IRoomRepository _rooms;
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;

        public RoomTypeService(
            IRoomTypeRepository repository,
            IRoomRepository rooms,
            IUserRepository users,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _rooms = rooms;
            _users = users;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<RoomType>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<RoomType?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        // CU-010 / RN-014
        public async Task AddAsync(RoomType roomType, int requestingUserId)
        {
            Validations.NotNull(roomType, "el tipo de habitación");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId, "RN-014");
            await ValidateAsync(roomType, null);

            await _repository.AddAsync(roomType);
            await _unitOfWork.SaveChangesAsync();
        }

        // CU-010 / RN-014. Existing reservations keep the price they were created with (ReservationDetail.UnitPrice).
        public async Task UpdateAsync(RoomType roomType, int requestingUserId)
        {
            Validations.NotNull(roomType, "el tipo de habitación");
            Validations.ValidId(roomType.RoomTypeId, "el tipo de habitación");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId, "RN-014");

            var existing = await _repository.GetByIdAsync(roomType.RoomTypeId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El tipo de habitación no existe.");

            await ValidateAsync(roomType, existing.RoomTypeId);

            existing.Name = roomType.Name;
            existing.MaxCapacity = roomType.MaxCapacity;
            existing.BaseRate = roomType.BaseRate;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int roomTypeId, int requestingUserId)
        {
            await Permissions.RequireAdministratorAsync(_users, requestingUserId, "RN-014");

            var existing = await _repository.GetByIdAsync(roomTypeId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El tipo de habitación no existe.");

            if (await _rooms.AnyRoomsOfTypeAsync(roomTypeId))
                throw new BusinessRuleException(ErrorCodes.Validation, "No se puede eliminar el tipo porque todavía hay habitaciones que lo usan.");

            _repository.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task ValidateAsync(RoomType roomType, int? existingId)
        {
            roomType.Name = Validations.MaxLength(Validations.Required(roomType.Name, "Nombre"), 60, "Nombre");

            if (roomType.MaxCapacity < 1 || roomType.MaxCapacity > 20)
                throw new BusinessRuleException(ErrorCodes.Validation, "La capacidad máxima debe estar entre 1 y 20 personas.");

            if (roomType.BaseRate <= 0)
                throw new BusinessRuleException(ErrorCodes.Validation, "La tarifa base debe ser mayor que cero.");

            if (await _repository.NameExistsAsync(roomType.Name, existingId))
                throw new BusinessRuleException(ErrorCodes.Validation, $"Ya existe un tipo de habitación llamado '{roomType.Name}'.");
        }
    }
}
