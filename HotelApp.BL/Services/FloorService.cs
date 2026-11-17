using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Services
{
    public class FloorService : IFloorService
    {
        private const int MinFloorNumber = 0;
        private const int MaxFloorNumber = 200;

        private readonly IFloorRepository _repository;
        private readonly IBlockRepository _blocks;
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;

        public FloorService(IFloorRepository repository, IBlockRepository blocks, IUserRepository users, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _blocks = blocks;
            _users = users;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Floor>> GetAllAsync(int? hotelId = null, int? blockId = null, bool includeInactive = false)
            => await _repository.GetFilteredAsync(hotelId, blockId, includeInactive);

        public async Task<Floor?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task AddAsync(Floor floor, int requestingUserId)
        {
            Validations.NotNull(floor, "el piso");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            floor.IsActive = true;
            await NormalizeAndValidateAsync(floor, null);

            await _repository.AddAsync(floor);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Floor floor, int requestingUserId)
        {
            Validations.NotNull(floor, "el piso");
            Validations.ValidId(floor.FloorId, "el piso");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var existing = await _repository.GetByIdAsync(floor.FloorId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El piso no existe.");

            await NormalizeAndValidateAsync(floor, existing.FloorId);

            // Moving a floor with rooms could duplicate room numbers in the destination hotel
            if (existing.BlockId != floor.BlockId && await _repository.HasRoomsAsync(existing.FloorId))
                throw new BusinessRuleException(ErrorCodes.Validation, "No se puede mover el piso a otro bloque porque ya tiene habitaciones registradas.");

            if (existing.IsActive && !floor.IsActive)
                await EnsureCanDeactivateAsync(existing);

            existing.BlockId = floor.BlockId;
            existing.Number = floor.Number;
            existing.Description = floor.Description;
            existing.IsActive = floor.IsActive;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int floorId, int requestingUserId)
        {
            Validations.ValidId(floorId, "el piso");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var floor = await _repository.GetByIdAsync(floorId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El piso no existe.");

            if (!floor.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El piso {floor.Number} ya está inactivo.");

            await EnsureCanDeactivateAsync(floor);

            floor.IsActive = false;
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureCanDeactivateAsync(Floor floor)
        {
            if (await _repository.HasActiveRoomsAsync(floor.FloorId))
                throw new BusinessRuleException(ErrorCodes.Validation,
                    $"No se puede desactivar el piso {floor.Number} porque todavía tiene habitaciones activas. Dé de baja primero sus habitaciones.");
        }

        private async Task NormalizeAndValidateAsync(Floor floor, int? existingId)
        {
            if (floor.Number < MinFloorNumber || floor.Number > MaxFloorNumber)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El número de piso debe estar entre {MinFloorNumber} y {MaxFloorNumber}.");

            floor.Description = string.IsNullOrWhiteSpace(floor.Description)
                ? null!
                : Validations.MaxLength(floor.Description.Trim(), 250, "Descripción");

            Validations.ValidId(floor.BlockId, "el bloque");
            var block = await _blocks.GetByIdAsync(floor.BlockId)
                ?? throw new BusinessRuleException(ErrorCodes.Validation, "El bloque seleccionado no existe.");

            if (!block.IsActive && floor.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El bloque '{block.Name}' está inactivo.");

            if (await _repository.NumberExistsInBlockAsync(floor.BlockId, floor.Number, existingId))
                throw new BusinessRuleException(ErrorCodes.Validation, $"El bloque '{block.Name}' ya tiene un piso {floor.Number}.");
        }
    }
}
