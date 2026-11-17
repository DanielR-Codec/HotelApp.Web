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
    public class BlockService : IBlockService
    {
        private readonly IBlockRepository _repository;
        private readonly IHotelRepository _hotels;
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;

        public BlockService(IBlockRepository repository, IHotelRepository hotels, IUserRepository users, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _hotels = hotels;
            _users = users;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Block>> GetAllAsync(int? hotelId = null, bool includeInactive = false)
            => await _repository.GetFilteredAsync(hotelId, includeInactive);

        public async Task<Block?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task AddAsync(Block block, int requestingUserId)
        {
            Validations.NotNull(block, "el bloque");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            block.IsActive = true;
            await NormalizeAndValidateAsync(block, null);

            await _repository.AddAsync(block);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Block block, int requestingUserId)
        {
            Validations.NotNull(block, "el bloque");
            Validations.ValidId(block.BlockId, "el bloque");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var existing = await _repository.GetByIdAsync(block.BlockId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El bloque no existe.");

            await NormalizeAndValidateAsync(block, existing.BlockId);

            // Moving a block with floors would also move its rooms to another hotel
            if (existing.HotelId != block.HotelId && await _repository.HasFloorsAsync(existing.BlockId))
                throw new BusinessRuleException(ErrorCodes.Validation, "No se puede mover el bloque a otro hotel porque ya tiene pisos registrados.");

            if (existing.IsActive && !block.IsActive)
                await EnsureCanDeactivateAsync(existing);

            existing.HotelId = block.HotelId;
            existing.Name = block.Name;
            existing.Description = block.Description;
            existing.IsActive = block.IsActive;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int blockId, int requestingUserId)
        {
            Validations.ValidId(blockId, "el bloque");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var block = await _repository.GetByIdAsync(blockId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El bloque no existe.");

            if (!block.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El bloque '{block.Name}' ya está inactivo.");

            await EnsureCanDeactivateAsync(block);

            block.IsActive = false;
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureCanDeactivateAsync(Block block)
        {
            if (await _repository.HasActiveFloorsAsync(block.BlockId))
                throw new BusinessRuleException(ErrorCodes.Validation,
                    $"No se puede desactivar el bloque '{block.Name}' porque todavía tiene pisos activos. Desactive primero sus pisos.");
        }

        private async Task NormalizeAndValidateAsync(Block block, int? existingId)
        {
            block.Name = Validations.MaxLength(Validations.Required(block.Name, "Nombre"), 60, "Nombre");
            block.Description = string.IsNullOrWhiteSpace(block.Description)
                ? null!
                : Validations.MaxLength(block.Description.Trim(), 250, "Descripción");

            Validations.ValidId(block.HotelId, "el hotel");
            var hotel = await _hotels.GetByIdAsync(block.HotelId)
                ?? throw new BusinessRuleException(ErrorCodes.Validation, "El hotel seleccionado no existe.");

            if (!hotel.IsActive && block.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El hotel '{hotel.Name}' está inactivo.");

            if (await _repository.NameExistsInHotelAsync(block.HotelId, block.Name, existingId))
                throw new BusinessRuleException(ErrorCodes.Validation, $"El hotel '{hotel.Name}' ya tiene un bloque llamado '{block.Name}'.");
        }
    }
}
