using System;
using System.Net.Mail;
using HotelApp.BL.Constants;
using HotelApp.BL.Exceptions;

namespace HotelApp.BL.Helpers
{
    internal static class Validations
    {
        /// <summary>Requires the screen to have sent the object (protects against null requests).</summary>
        public static T NotNull<T>(T? value, string description) where T : class
        {
            return value ?? throw new BusinessRuleException(ErrorCodes.Validation, $"No se recibieron los datos de {description}.");
        }

        /// <summary>Requires a valid id (greater than zero).</summary>
        public static int ValidId(int id, string description)
        {
            if (id <= 0)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El identificador de {description} no es válido.");

            return id;
        }

        /// <summary>Requires the text not to exceed the maximum length of its column.</summary>
        public static string MaxLength(string value, int maxLength, string fieldName)
        {
            if (value is not null && value.Length > maxLength)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El campo '{fieldName}' no puede pasar de {maxLength} caracteres.");

            return value!;
        }

        /// <summary>Requires the text to be non-empty and returns it trimmed.</summary>
        public static string Required(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessRuleException(ErrorCodes.Validation, $"El campo '{fieldName}' es obligatorio.");

            return value.Trim();
        }

        /// <summary>Requires a well-formed email and returns it in lowercase.</summary>
        public static string ValidEmail(string? email)
        {
            var clean = Required(email, "Correo").ToLowerInvariant();

            if (!MailAddress.TryCreate(clean, out var address) || address.Address != clean)
                throw new BusinessRuleException(ErrorCodes.Validation, $"El correo '{clean}' no tiene un formato válido.");

            return clean;
        }

        /// <summary>RN-002: is the person at least the minimum age on the given date?</summary>
        public static bool IsAdult(DateTime birthDate, DateTime today)
        {
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.Date.AddYears(-age))
                age--;

            return age >= HotelRules.MinPrimaryGuestAge;
        }

        /// <summary>RN-003 and consistent dates: check-out is at least one day after check-in and check-in is not in the past.</summary>
        public static void ValidateStayDates(DateTime checkIn, DateTime checkOut, DateTime now)
        {
            if (checkIn.Date < now.Date)
                throw new BusinessRuleException(ErrorCodes.Validation, "La fecha de llegada no puede estar en el pasado.");

            if (checkOut.Date < checkIn.Date.AddDays(1))
                throw new BusinessRuleException("RN-003", "La fecha de salida debe ser al menos un día después de la llegada (no existen reservas de 0 noches).");
        }
    }
}
