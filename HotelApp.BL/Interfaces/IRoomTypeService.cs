using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    /// <summary>
    /// CU-010 / RF-002. Creating, changing rates or deleting types is only for the Administrator (RN-014),
    /// so these operations take the id of the user who runs them.
    /// </summary>
    public interface IRoomTypeService
    {
        Task<IEnumerable<RoomType>> GetAllAsync();
        Task<RoomType?> GetByIdAsync(int id);
        Task AddAsync(RoomType roomType, int requestingUserId);
        Task UpdateAsync(RoomType roomType, int requestingUserId);
        Task DeleteAsync(int roomTypeId, int requestingUserId);
    }
}
