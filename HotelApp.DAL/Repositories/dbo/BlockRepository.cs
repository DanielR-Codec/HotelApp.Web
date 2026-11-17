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
    public class BlockRepository : BaseRepository<Block>, IBlockRepository
    {
        public BlockRepository(HotelContext context, ILogger<BlockRepository> logger) : base(context, logger) { }

        public Task<IEnumerable<Block>> GetFilteredAsync(int? hotelId, bool includeInactive)
        {
            return ExecuteAsync<IEnumerable<Block>>(async () =>
                await _dbSet.Where(b => (hotelId == null || b.HotelId == hotelId)
                                     && (includeInactive || b.IsActive))
                            .OrderBy(b => b.Name)
                            .ToListAsync(),
                nameof(GetFilteredAsync));
        }

        public Task<bool> NameExistsInHotelAsync(int hotelId, string name, int? excludeBlockId = null)
        {
            if (hotelId <= 0 || string.IsNullOrWhiteSpace(name))
                return Task.FromResult(false);

            var clean = name.Trim().ToLower();
            return ExecuteAsync(async () =>
                await _dbSet.AnyAsync(b => b.HotelId == hotelId
                                        && b.Name.ToLower() == clean
                                        && (excludeBlockId == null || b.BlockId != excludeBlockId)),
                nameof(NameExistsInHotelAsync));
        }

        public Task<bool> HasFloorsAsync(int blockId)
        {
            if (blockId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
                await _context.Floors.AnyAsync(f => f.BlockId == blockId),
                nameof(HasFloorsAsync));
        }

        public Task<bool> HasActiveFloorsAsync(int blockId)
        {
            if (blockId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
                await _context.Floors.AnyAsync(f => f.BlockId == blockId && f.IsActive),
                nameof(HasActiveFloorsAsync));
        }
    }
}
