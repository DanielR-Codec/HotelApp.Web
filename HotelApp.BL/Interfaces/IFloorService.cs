using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Interfaces
{
    /// <summary>Floor management. Reading is open to any user; changes are for the Administrator.</summary>
    public interface IFloorService
    {
        Task<IEnumerable<Floor>> GetAllAsync(int? hotelId = null, int? blockId = null, bool includeInactive = false);
        Task<Floor?> GetByIdAsync(int id);
        Task AddAsync(Floor floor, int requestingUserId);

        /// <summary>Changes the data of the floor and whether it is active. A floor that already has rooms cannot move to another block.</summary>
        Task UpdateAsync(Floor floor, int requestingUserId);

        /// <summary>Soft delete. Not allowed while the floor still has active rooms.</summary>
        Task DeleteAsync(int floorId, int requestingUserId);
    }
}
