using System.Threading.Tasks;
using HotelApp.DAL.Base;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelApp.DAL.Repositories.dbo
{
    public class RoomTypeRepository : BaseRepository<RoomType>, IRoomTypeRepository
    {
        public RoomTypeRepository(HotelContext context, ILogger<RoomTypeRepository> logger) : base(context, logger) { }

        public Task<bool> NameExistsAsync(string name, int? excludeRoomTypeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Task.FromResult(false);

            var clean = name.Trim().ToLower();
            return ExecuteAsync(async () =>
                await _dbSet.AnyAsync(t => t.Name.ToLower() == clean
                                        && (excludeRoomTypeId == null || t.RoomTypeId != excludeRoomTypeId)),
                nameof(NameExistsAsync));
        }
    }
}
