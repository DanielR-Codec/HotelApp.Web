using System;

namespace HotelApp.DAL.Exceptions
{
    public enum DataErrorType
    {
        Unknown,
        DuplicateKey,
        ForeignKey,
        RequiredField,
        FieldTooLong,
        Concurrency,
        Timeout,
        Connection
    }

    /// <summary>
    /// A database error translated into a message the user can understand.
    /// The original exception is kept in InnerException (and it is logged) for the developer.
    /// </summary>
    public class DataAccessException : HotelAppException
    {
        public DataErrorType ErrorType { get; }

        /// <summary>Operation that failed (for example "RoomRepository.GetByIdAsync").</summary>
        public string Operation { get; }

        public DataAccessException(DataErrorType errorType, string message, string operation, Exception innerException = null)
            : base($"DB_{errorType.ToString().ToUpperInvariant()}", message, innerException)
        {
            ErrorType = errorType;
            Operation = operation;
        }
    }
}
