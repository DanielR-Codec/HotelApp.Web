using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IFloorRepository : IBaseRepository<Floor>
    {
        /// <summary>Floors ordered by number; optionally only those of one hotel and/or one block.</summary>
        Task<IEnumerable<Floor>> GetFilteredAsync(int? hotelId, int? blockId, bool includeInactive);

        Task<bool> NumberExistsInBlockAsync(int blockId, int number, int? excludeFloorId = null);
        Task<bool> HasRoomsAsync(int floorId);
        Task<bool> HasActiveRoomsAsync(int floorId);

        /// <summary>Hotel the floor belongs to (through its block), or null if the floor does not exist.</summary>
        Task<int?> GetHotelIdAsync(int floorId);
    }
}
