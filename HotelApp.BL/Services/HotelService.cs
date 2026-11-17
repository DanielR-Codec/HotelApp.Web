using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Services
{
    public class HotelService : IHotelService
    {
        private static readonly Regex PhoneFormat = new(@"^[0-9+()\-\s]{7,20}$", RegexOptions.Compiled);

        private readonly IHotelRepository _repository;
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;

        public HotelService(IHotelRepository repository, IUserRepository users, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _users = users;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Hotel>> GetAllAsync(bool includeInactive = false) => await _repository.GetFilteredAsync(includeInactive);

        public async Task<Hotel?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task AddAsync(Hotel hotel, int requestingUserId)
        {
            Validations.NotNull(hotel, "el hotel");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);
            await NormalizeAndValidateAsync(hotel, null);

            hotel.IsActive = true;

            await _repository.AddAsync(hotel);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Hotel hotel, int requestingUserId)
        {
            Validations.NotNull(hotel, "el hotel");
            Validations.ValidId(hotel.HotelId, "el hotel");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var existing = await _repository.GetByIdAsync(hotel.HotelId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El hotel no existe.");

            await NormalizeAndValidateAsync(hotel, existing.HotelId);

            if (existing.IsActive && !hotel.IsActive)
                await EnsureCanDeactivateAsync(existing);

            existing.Name = hotel.Name;
            existing.Address = hotel.Address;
            existing.City = hotel.City;
            existing.Phone = hotel.Phone;
            existing.Email = hotel.Email;
            existing.IsActive = hotel.IsActive;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(int hotelId, int requestingUserId)
        {
            Validations.ValidId(hotelId, "el hotel");
            await Permissions.RequireAdministratorAsync(_users, requestingUserId);

            var hotel = await _repository.GetByIdAsync(hotelId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El hotel no existe.");

            if (!hotel.IsActive)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El hotel '{hotel.Name}' ya está inactivo.");

            await EnsureCanDeactivateAsync(hotel);

            hotel.IsActive = false;
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureCanDeactivateAsync(Hotel hotel)
        {
            if (await _repository.HasActiveBlocksAsync(hotel.HotelId))
                throw new BusinessRuleException(ErrorCodes.Validation,
                    $"No se puede desactivar el hotel '{hotel.Name}' porque todavía tiene bloques activos. Desactive primero sus bloques.");
        }

        private async Task NormalizeAndValidateAsync(Hotel hotel, int? existingId)
        {
            hotel.Name = Validations.MaxLength(Validations.Required(hotel.Name, "Nombre"), 100, "Nombre");
            hotel.Address = Validations.MaxLength(Validations.Required(hotel.Address, "Dirección"), 200, "Dirección");
            hotel.City = Validations.MaxLength(Validations.Required(hotel.City, "Ciudad"), 80, "Ciudad");
            hotel.Phone = Validations.MaxLength(Validations.Required(hotel.Phone, "Teléfono"), 30, "Teléfono");

            if (!PhoneFormat.IsMatch(hotel.Phone))
                throw new BusinessRuleException(ErrorCodes.Validation, "El teléfono solo puede tener números, espacios y los símbolos + ( ) - (entre 7 y 20 caracteres).");

            // The email is optional
            hotel.Email = string.IsNullOrWhiteSpace(hotel.Email)
                ? null!
                : Validations.MaxLength(Validations.ValidEmail(hotel.Email), 150, "Correo");

            if (await _repository.NameExistsAsync(hotel.Name, existingId))
                throw new BusinessRuleException(ErrorCodes.Validation, $"Ya existe un hotel llamado '{hotel.Name}'.");
        }
    }
}
