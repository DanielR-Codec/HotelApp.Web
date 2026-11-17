using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IBlockRepository : IBaseRepository<Block>
    {
        /// <summary>Blocks ordered by name; optionally only those of one hotel.</summary>
        Task<IEnumerable<Block>> GetFilteredAsync(int? hotelId, bool includeInactive);

        Task<bool> NameExistsInHotelAsync(int hotelId, string name, int? excludeBlockId = null);
        Task<bool> HasFloorsAsync(int blockId);
        Task<bool> HasActiveFloorsAsync(int blockId);
    }
}
