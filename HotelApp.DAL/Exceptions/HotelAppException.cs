using System;

namespace HotelApp.DAL.Exceptions
{
    /// <summary>
    /// Base of every controlled exception of the application (data, validation and business rules).
    /// The Web layer only needs to catch this type to show <see cref="Exception.Message"/> to the user;
    /// any other exception is an unexpected bug.
    /// </summary>
    public abstract class HotelAppException : Exception
    {
        /// <summary>Machine-readable code: "VALIDATION", "DB_DUPLICATEKEY", "RN-001", "NOT_FOUND"...</summary>
        public string Code { get; }

        protected HotelAppException(string code, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Code = code;
        }
    }
}
