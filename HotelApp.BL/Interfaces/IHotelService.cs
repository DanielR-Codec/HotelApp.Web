using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Interfaces
{
    /// <summary>Hotel management. Reading is open to any user; creating, editing and deactivating is for the Administrator.</summary>
    public interface IHotelService
    {
        Task<IEnumerable<Hotel>> GetAllAsync(bool includeInactive = false);
        Task<Hotel?> GetByIdAsync(int id);
        Task AddAsync(Hotel hotel, int requestingUserId);

        /// <summary>Changes the data of the hotel and whether it is active.</summary>
        Task UpdateAsync(Hotel hotel, int requestingUserId);

        /// <summary>Soft delete. Not allowed while the hotel still has active blocks.</summary>
        Task DeleteAsync(int hotelId, int requestingUserId);
    }
}
