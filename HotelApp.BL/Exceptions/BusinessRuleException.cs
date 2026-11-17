using HotelApp.DAL.Exceptions;

namespace HotelApp.BL.Exceptions
{
    /// <summary>
    /// Thrown when an operation breaks a business rule or a validation.
    /// It inherits from HotelAppException, like the DAL errors (DataAccessException / EntityValidationException),
    /// so the Web layer can catch all controlled errors with a single catch and show the message to the user as-is.
    /// Code: "RN-001", "CU-008", "VALIDATION", "AUTH", "FORBIDDEN", "NOT_FOUND"...
    /// </summary>
    public class BusinessRuleException : HotelAppException
    {
        public BusinessRuleException(string code, string message) : base(code, message)
        {
        }
    }
}
