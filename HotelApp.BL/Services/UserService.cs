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
using HotelApp.DAL.Constants;

namespace HotelApp.BL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<User>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<User?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<bool> AnyUsersAsync() => await _repository.AnyExistsAsync();

        // CU-001 / RF-011 / RNF-002
        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
                return null;

            var user = await _repository.GetByEmailAsync(email.Trim().ToLowerInvariant());

            if (user is null || !user.IsActive)
                return null;

            return PasswordHasher.Verify(password, user.PasswordHash) ? user : null;
        }

        // CU-013
        public async Task<User> CreateAsync(string name, string email, string password, string role, int? requestingUserId)
        {
            var anyUsers = await _repository.AnyExistsAsync();

            if (anyUsers)
            {
                if (requestingUserId is null)
                    throw new BusinessRuleException(ErrorCodes.Forbidden, "Debe iniciar sesión como Administrador para crear usuarios.");

                await Permissions.RequireAdministratorAsync(_repository, requestingUserId.Value);
            }

            name = Validations.MaxLength(Validations.Required(name, "Nombre"), 100, "Nombre");
            email = Validations.MaxLength(Validations.ValidEmail(email), 150, "Correo");
            ValidatePassword(password);

            // Bootstrap: the first user of the system is always an Administrator
            role = anyUsers ? ValidateRole(role) : UserRole.Administrator;

            if (await _repository.GetByEmailAsync(email) is not null)
                throw new BusinessRuleException(ErrorCodes.Validation, $"Ya existe un usuario con el correo '{email}'.");

            var user = new User
            {
                Name = name,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                Role = role,
                IsActive = true
            };

            await _repository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            return user;
        }

        // CU-013
        public async Task UpdateAsync(int userId, string name, string role, bool isActive, int requestingUserId)
        {
            await Permissions.RequireAdministratorAsync(_repository, requestingUserId);

            var user = await _repository.GetByIdAsync(userId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El usuario no existe.");

            name = Validations.MaxLength(Validations.Required(name, "Nombre"), 100, "Nombre");
            role = ValidateRole(role);

            // Prevents the last administrator from locking themselves out
            if (userId == requestingUserId && (!isActive || role != UserRole.Administrator))
                throw new BusinessRuleException(ErrorCodes.Validation, "No puede desactivarse ni quitarse el perfil de Administrador a sí mismo.");

            user.Name = name;
            user.Role = role;
            user.IsActive = isActive;
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _repository.GetByIdAsync(userId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El usuario no existe.");

            if (!PasswordHasher.Verify(currentPassword ?? string.Empty, user.PasswordHash))
                throw new BusinessRuleException(ErrorCodes.Auth, "La clave actual no es correcta.");

            ValidatePassword(newPassword);

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            await _unitOfWork.SaveChangesAsync();
        }

        private static void ValidatePassword(string? password)
        {
            if (string.IsNullOrEmpty(password) || password.Length < HotelRules.MinPasswordLength)
                throw new BusinessRuleException(ErrorCodes.Validation, $"La clave debe tener al menos {HotelRules.MinPasswordLength} caracteres.");
        }

        private static string ValidateRole(string? role)
        {
            var clean = Validations.Required(role, "Rol");

            var valid = UserRole.All.FirstOrDefault(r => string.Equals(r, clean, StringComparison.OrdinalIgnoreCase));
            if (valid is null)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El rol '{clean}' no es válido. Roles permitidos: {string.Join(", ", UserRole.All)}.");

            return valid;
        }
    }
}
