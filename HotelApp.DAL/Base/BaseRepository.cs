using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelApp.DAL.Base
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly HotelContext _context;
        protected readonly DbSet<T> _dbSet;
        protected readonly ILogger _logger;

        public BaseRepository(HotelContext context, ILogger<BaseRepository<T>> logger)
            : this(context, (ILogger)logger)
        {
        }

        protected BaseRepository(HotelContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
            _logger = logger ?? NullLogger.Instance;
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return ExecuteAsync<IEnumerable<T>>(async () => await _dbSet.ToListAsync(), nameof(GetAllAsync));
        }

        public Task<T> GetByIdAsync(int id)
        {
            if (id <= 0)
                return Task.FromResult<T>(null);

            return ExecuteAsync(async () => await _dbSet.FindAsync(id), nameof(GetByIdAsync));
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await GetByIdAsync(id) is not null;
        }

        public async Task AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            EntityValidator.Validate(entity);

            await ExecuteAsync(async () =>
            {
                await _dbSet.AddAsync(entity);
                return true;
            }, nameof(AddAsync));
        }

        public void Update(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            EntityValidator.Validate(entity);

            Execute(() => _dbSet.Update(entity), nameof(Update));
        }

        public void Remove(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            Execute(() => _dbSet.Remove(entity), nameof(Remove));
        }

        /// <summary>
        /// Runs a database operation. Database errors are logged and translated into a DataAccessException
        /// with a message the user can understand; any other exception (a bug) is not hidden.
        /// Every query of the derived repositories goes through here.
        /// </summary>
        protected async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> operation, string operationName)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (DataErrorTranslator.IsDataException(ex))
            {
                _logger.LogError(ex, "Data access error in {Repository}.{Operation}.", GetType().Name, operationName);
                throw DataErrorTranslator.Translate(ex, $"{GetType().Name}.{operationName}");
            }
        }

        protected void Execute(Action operation, string operationName)
        {
            try
            {
                operation();
            }
            catch (Exception ex) when (DataErrorTranslator.IsDataException(ex))
            {
                _logger.LogError(ex, "Data access error in {Repository}.{Operation}.", GetType().Name, operationName);
                throw DataErrorTranslator.Translate(ex, $"{GetType().Name}.{operationName}");
            }
        }
    }
}
