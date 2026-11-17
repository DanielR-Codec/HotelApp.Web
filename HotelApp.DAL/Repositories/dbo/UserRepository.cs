using System.Threading.Tasks;
using HotelApp.DAL.Base;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelApp.DAL.Repositories.dbo
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(HotelContext context, ILogger<UserRepository> logger) : base(context, logger) { }

        public Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Task.FromResult<User>(null);

            var clean = email.Trim().ToLowerInvariant();
            return ExecuteAsync(async () =>
                await _dbSet.FirstOrDefaultAsync(u => u.Email == clean),
                nameof(GetByEmailAsync));
        }

        public Task<bool> AnyExistsAsync()
        {
            return ExecuteAsync(async () => await _dbSet.AnyAsync(), nameof(AnyExistsAsync));
        }
    }
}
