using System;
using System.Data;
using System.Threading.Tasks;

namespace HotelApp.DAL.Interfaces
{
    /// <summary>
    /// Repositories only track changes in memory (Add/Update/Remove).
    /// To actually persist them to the database call SaveChangesAsync,
    /// or run everything inside a transaction (RNF-006).
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Runs the operation inside a SQL transaction. If the operation throws, the transaction is rolled back;
        /// if it completes, changes are saved and the transaction is committed.
        /// </summary>
        Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}
