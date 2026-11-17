using System;
using System.Data;
using System.Threading.Tasks;
using HotelApp.DAL.Context;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace HotelApp.DAL.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HotelContext _context;
        private readonly ILogger<UnitOfWork> _logger;

        public UnitOfWork(HotelContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                // HotelContext validates the fields and translates database errors
                return await _context.SaveChangesAsync();
            }
            catch (HotelAppException ex)
            {
                _logger.LogWarning(ex, "Changes could not be saved: {Message}", ex.Message);
                throw;
            }
        }

        public Task ExecuteInTransactionAsync(Func<Task> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ArgumentNullException.ThrowIfNull(operation);

            return ExecuteInTransactionAsync<bool>(async () =>
            {
                await operation();
                return true;
            }, isolationLevel);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            ArgumentNullException.ThrowIfNull(operation);

            // Already inside a transaction: join it instead of trying to open a nested one
            if (_context.Database.CurrentTransaction is not null)
                return await operation();

            IDbContextTransaction transaction;
            try
            {
                transaction = await _context.Database.BeginTransactionAsync(isolationLevel);
            }
            catch (Exception ex) when (DataErrorTranslator.IsDataException(ex))
            {
                _logger.LogError(ex, "Could not start a database transaction.");
                throw DataErrorTranslator.Translate(ex, "UnitOfWork.BeginTransaction");
            }

            await using (transaction)
            {
                try
                {
                    var result = await operation();
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return result;
                }
                catch (Exception ex)
                {
                    // RNF-006: if something fails halfway, nothing is left half-done
                    await SafeRollbackAsync(transaction);

                    if (DataErrorTranslator.IsDataException(ex))
                    {
                        _logger.LogError(ex, "Transaction rolled back because of a database error.");
                        throw DataErrorTranslator.Translate(ex, "UnitOfWork.ExecuteInTransaction");
                    }

                    if (ex is HotelAppException)
                        _logger.LogWarning("Transaction rolled back: {Message}", ex.Message);
                    else
                        _logger.LogError(ex, "Transaction rolled back because of an unexpected error.");

                    throw;
                }
            }
        }

        private async Task SafeRollbackAsync(IDbContextTransaction transaction)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackError)
            {
                // A failed rollback must not hide the original error
                _logger.LogError(rollbackError, "Rollback failed.");
            }
            finally
            {
                // Discard the pending changes so they are not saved later by accident
                _context.ChangeTracker.Clear();
            }
        }
    }
}
