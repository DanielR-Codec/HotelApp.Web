using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.DAL.Interfaces
{
    /// <summary>
    /// Common operations of every repository.
    /// Database errors are thrown as DataAccessException and invalid fields as EntityValidationException
    /// (both inherit from HotelAppException), never as raw EF Core / SQL Server exceptions.
    /// </summary>
    public interface IBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>Returns null if the id is not valid (&lt;= 0) or the record does not exist.</summary>
        Task<T> GetByIdAsync(int id);

        Task<bool> ExistsAsync(int id);

        /// <summary>Validates the entity's fields and marks it to be inserted on the next SaveChanges.</summary>
        Task AddAsync(T entity);

        /// <summary>Validates the entity's fields and marks it to be updated on the next SaveChanges.</summary>
        void Update(T entity);

        void Remove(T entity);
    }
}
