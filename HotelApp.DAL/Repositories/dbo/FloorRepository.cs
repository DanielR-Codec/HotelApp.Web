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
    public class FloorRepository : BaseRepository<Floor>, IFloorRepository
    {
        public FloorRepository(HotelContext context, ILogger<FloorRepository> logger) : base(context, logger) { }

        public Task<IEnumerable<Floor>> GetFilteredAsync(int? hotelId, int? blockId, bool includeInactive)
        {
            return ExecuteAsync<IEnumerable<Floor>>(async () =>
            {
                var query = from f in _context.Floors
                            join b in _context.Blocks on f.BlockId equals b.BlockId
                            where (hotelId == null || b.HotelId == hotelId)
                               && (blockId == null || f.BlockId == blockId)
                               && (includeInactive || f.IsActive)
                            orderby b.Name, f.Number
                            select f;

                return await query.ToListAsync();
            }, nameof(GetFilteredAsync));
        }

        public Task<bool> NumberExistsInBlockAsync(int blockId, int number, int? excludeFloorId = null)
        {
            if (blockId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
                await _dbSet.AnyAsync(f => f.BlockId == blockId
                                        && f.Number == number
                                        && (excludeFloorId == null || f.FloorId != excludeFloorId)),
                nameof(NumberExistsInBlockAsync));
        }

        public Task<bool> HasRoomsAsync(int floorId)
        {
            if (floorId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
                await _context.Rooms.AnyAsync(r => r.FloorId == floorId),
                nameof(HasRoomsAsync));
        }

        public Task<bool> HasActiveRoomsAsync(int floorId)
        {
            if (floorId <= 0)
                return Task.FromResult(false);

            return ExecuteAsync(async () =>
                await _context.Rooms.AnyAsync(r => r.FloorId == floorId && r.IsActive),
                nameof(HasActiveRoomsAsync));
        }

        public Task<int?> GetHotelIdAsync(int floorId)
        {
            if (floorId <= 0)
                return Task.FromResult<int?>(null);

            return ExecuteAsync(async () =>
                await (from f in _context.Floors
                       join b in _context.Blocks on f.BlockId equals b.BlockId
                       where f.FloorId == floorId
                       select (int?)b.HotelId).FirstOrDefaultAsync(),
                nameof(GetHotelIdAsync));
        }
    }
}
