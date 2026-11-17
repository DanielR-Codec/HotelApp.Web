using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.DAL.Base;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelApp.DAL.Repositories.dbo
{
    public class HotelRepository : BaseRepository<Hotel>, IHotelRepository
    {
        public HotelRepository(HotelContext context, ILogger<HotelRepository> logger) : base(context, logger) { }

        public Task<IEnumerable<Hotel>> GetFilteredAsync(bool includeInactive)
        {
            return ExecuteAsync<IEnumerable<Hotel>>(async () =>
                await _dbSet.Where(h => includeInactive || h.IsActive)
                            .OrderBy(h => h.Name)
                            .ToListAsync(),
                nameof(GetFilteredAsync));
        }

        public Task<bool> NameExistsAsync(string name, int? excludeHotelId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Task.FromResult(false);

            var clean = name.Trim().ToLower();
            return ExecuteAsync(async () =>
                await _dbSet.AnyAsync(h => h.Name.ToLower() == clean
                                        && (excludeHotelId == null || h.HotelId != excludeHotelId)),
                nameof(NameExistsAsync));
        }

        public Task<bool> HasActiveBlocksAsync(int hotelId)
        {
            if (hotelId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
                await _context.Blocks.AnyAsync(b => b.HotelId == hotelId && b.IsActive),
                nameof(HasActiveBlocksAsync));
        }
    }
}
