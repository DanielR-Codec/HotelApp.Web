using System.Collections.Generic;
using System.Linq;

namespace HotelApp.DAL.Exceptions
{
    /// <summary>Thrown when an entity does not pass the field validations of its model (DataAnnotations / IValidatableObject).</summary>
    public class EntityValidationException : HotelAppException
    {
        public IReadOnlyList<string> Errors { get; }

        public EntityValidationException(IEnumerable<string> errors)
            : this(errors.ToList())
        {
        }

        private EntityValidationException(List<string> errors)
            : base("VALIDATION", string.Join(" ", errors))
        {
            Errors = errors;
        }
    }
}
