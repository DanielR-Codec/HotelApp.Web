using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using HotelApp.DAL.Exceptions;

namespace HotelApp.DAL.Base
{
    /// <summary>
    /// Runs the field validations declared on the models (DataAnnotations + IValidatableObject).
    /// It is used when an entity is added/updated and again right before SaveChanges, so invalid data never reaches SQL Server.
    /// </summary>
    public static class EntityValidator
    {
        public static IReadOnlyList<string> GetErrors(object entity)
        {
            if (entity is null)
                return new[] { "No se recibieron los datos a guardar." };

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(entity, new ValidationContext(entity), results, validateAllProperties: true);

            return results
                .Select(r => r.ErrorMessage)
                .Where(m => !string.IsNullOrWhiteSpace(m))
                .Distinct()
                .ToList();
        }

        /// <summary>Throws <see cref="EntityValidationException"/> if the entity has invalid fields.</summary>
        public static void Validate(object entity)
        {
            var errors = GetErrors(entity);
            if (errors.Count > 0)
                throw new EntityValidationException(errors);
        }
    }
}
