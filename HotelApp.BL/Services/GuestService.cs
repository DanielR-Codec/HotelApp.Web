using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Exceptions;
using HotelApp.BL.Helpers;

namespace HotelApp.BL.Services
{
    public class GuestService : IGuestService
    {
        private static readonly Regex PhoneFormat = new(@"^[0-9+()\-\s]{7,20}$", RegexOptions.Compiled);

        private readonly IGuestRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public GuestService(IGuestRepository repository, IUnitOfWork unitOfWork, TimeProvider timeProvider)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<IEnumerable<Guest>> GetAllAsync() => await _repository.GetAllAsync();

        public async Task<Guest?> GetByIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<Guest>> SearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return await _repository.GetAllAsync();

            return await _repository.SearchAsync(text.Trim());
        }

        public async Task<Guest?> GetByDocumentAsync(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                return null;

            return await _repository.GetByDocumentAsync(document.Trim());
        }

        // CU-002
        public async Task AddAsync(Guest guest)
        {
            Validations.NotNull(guest, "el huésped");
            NormalizeAndValidate(guest);

            if (await _repository.GetByDocumentAsync(guest.IdentityDocument) is not null)
                throw new BusinessRuleException(ErrorCodes.Validation, $"Ya existe un huésped registrado con el documento '{guest.IdentityDocument}'.");

            await _repository.AddAsync(guest);
            await _unitOfWork.SaveChangesAsync();
        }

        // CU-003
        public async Task UpdateAsync(Guest guest)
        {
            Validations.NotNull(guest, "el huésped");
            Validations.ValidId(guest.GuestId, "el huésped");

            var existing = await _repository.GetByIdAsync(guest.GuestId)
                ?? throw new BusinessRuleException(ErrorCodes.NotFound, "El huésped no existe.");

            NormalizeAndValidate(guest);

            var sameDocument = await _repository.GetByDocumentAsync(guest.IdentityDocument);
            if (sameDocument is not null && sameDocument.GuestId != existing.GuestId)
                throw new BusinessRuleException(ErrorCodes.Validation, $"Otro huésped ya tiene el documento '{guest.IdentityDocument}'.");

            existing.IdentityDocument = guest.IdentityDocument;
            existing.FirstName = guest.FirstName;
            existing.LastName = guest.LastName;
            existing.Phone = guest.Phone;
            existing.Email = guest.Email;
            existing.BirthDate = guest.BirthDate;

            await _unitOfWork.SaveChangesAsync();
        }

        private void NormalizeAndValidate(Guest guest)
        {
            guest.IdentityDocument = Validations.Required(guest.IdentityDocument, "Documento de identidad");
            guest.FirstName = Validations.Required(guest.FirstName, "Nombres");
            guest.LastName = Validations.Required(guest.LastName, "Apellidos");
            guest.Phone = Validations.Required(guest.Phone, "Teléfono");
            guest.Email = Validations.ValidEmail(guest.Email);

            Validations.MaxLength(guest.IdentityDocument, 30, "Documento de identidad");
            Validations.MaxLength(guest.FirstName, 100, "Nombres");
            Validations.MaxLength(guest.LastName, 100, "Apellidos");
            Validations.MaxLength(guest.Email, 150, "Correo");

            if (!PhoneFormat.IsMatch(guest.Phone))
                throw new BusinessRuleException(ErrorCodes.Validation, "El teléfono solo puede tener números, espacios y los símbolos + ( ) - (entre 7 y 20 caracteres).");

            // RN-002: the primary guest must be an adult
            var today = _timeProvider.GetLocalNow().DateTime;
            if (guest.BirthDate == default || guest.BirthDate.Date > today.Date)
                throw new BusinessRuleException(ErrorCodes.Validation, "La fecha de nacimiento no es válida.");

            if (!Validations.IsAdult(guest.BirthDate, today))
                throw new BusinessRuleException("RN-002", "El huésped titular debe ser mayor de edad.");
        }
    }
}
