using System;
using System.Data.Common;
using HotelApp.DAL.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HotelApp.DAL.Base
{
    /// <summary>Turns low-level database exceptions (EF Core / SQL Server) into a <see cref="DataAccessException"/>.</summary>
    internal static class DataErrorTranslator
    {
        /// <summary>true if the exception comes from the database and must be translated.</summary>
        public static bool IsDataException(Exception ex)
        {
            return ex is DbException
                || ex is DbUpdateException
                || ex is TimeoutException
                // EF Core wraps some connection/transient failures in an InvalidOperationException
                || (ex is InvalidOperationException && Find<DbException>(ex) is not null);
        }

        public static DataAccessException Translate(Exception ex, string operation)
        {
            if (ex is DataAccessException alreadyTranslated)
                return alreadyTranslated;

            if (ex is DbUpdateConcurrencyException)
                return new DataAccessException(DataErrorType.Concurrency,
                    "Otro usuario modificó este registro al mismo tiempo. Recargue los datos e intente de nuevo.", operation, ex);

            var sql = Find<SqlException>(ex);
            if (sql is not null)
            {
                switch (sql.Number)
                {
                    case 2601: // duplicate key in unique index
                    case 2627: // unique constraint / primary key violation
                        return new DataAccessException(DataErrorType.DuplicateKey,
                            "Ya existe un registro con esos datos (valor duplicado).", operation, ex);

                    case 547:  // FOREIGN KEY / CHECK constraint
                        return new DataAccessException(DataErrorType.ForeignKey,
                            "La operación no se puede completar porque el registro está relacionado con otros datos o hace referencia a uno que no existe.", operation, ex);

                    case 515:  // NULL in a NOT NULL column
                        return new DataAccessException(DataErrorType.RequiredField,
                            "Falta un campo obligatorio.", operation, ex);

                    case 2628: // string or binary data would be truncated
                    case 8152:
                        return new DataAccessException(DataErrorType.FieldTooLong,
                            "Uno de los campos excede la longitud permitida.", operation, ex);

                    case 1205: // deadlock victim (possible with SERIALIZABLE transactions)
                        return new DataAccessException(DataErrorType.Concurrency,
                            "Otra operación estaba usando los mismos datos al mismo tiempo. Intente de nuevo.", operation, ex);

                    case -2:   // command timeout
                        return new DataAccessException(DataErrorType.Timeout,
                            "La base de datos tardó demasiado en responder. Intente de nuevo.", operation, ex);

                    case -1:    // connection error
                    case 2:     // server not found / not accessible
                    case 26:    // error locating server/instance
                    case 40:    // could not open a connection
                    case 53:    // network path not found
                    case 233:   // no process on the other end of the pipe
                    case 4060:  // cannot open database
                    case 18456: // login failed
                    case 10053: // connection aborted (Winsock)
                    case 10054: // connection reset by peer (Winsock)
                    case 10060: // connection timed out (Winsock)
                    case 10061: // connection refused (Winsock)
                    case 11001: // host not found (Winsock)
                        return new DataAccessException(DataErrorType.Connection,
                            "No se pudo conectar con la base de datos.", operation, ex);
                }
            }

            if (ex is TimeoutException)
                return new DataAccessException(DataErrorType.Timeout,
                    "La base de datos tardó demasiado en responder. Intente de nuevo.", operation, ex);

            if (ex is DbUpdateException)
                return new DataAccessException(DataErrorType.Unknown,
                    "No se pudieron guardar los cambios en la base de datos.", operation, ex);

            return new DataAccessException(DataErrorType.Unknown,
                "Ocurrió un error al acceder a la base de datos.", operation, ex);
        }

        private static T Find<T>(Exception ex) where T : Exception
        {
            for (var current = ex; current is not null; current = current.InnerException)
            {
                if (current is T match)
                    return match;
            }

            return null;
        }
    }
}
