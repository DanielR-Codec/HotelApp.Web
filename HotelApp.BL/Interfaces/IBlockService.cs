using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Interfaces
{
    /// <summary>Block (building / tower / wing) management. Reading is open to any user; changes are for the Administrator.</summary>
    public interface IBlockService
    {
        Task<IEnumerable<Block>> GetAllAsync(int? hotelId = null, bool includeInactive = false);
        Task<Block?> GetByIdAsync(int id);
        Task AddAsync(Block block, int requestingUserId);

        /// <summary>Changes the data of the block and whether it is active. A block that already has floors cannot move to another hotel.</summary>
        Task UpdateAsync(Block block, int requestingUserId);

        /// <summary>Soft delete. Not allowed while the block still has active floors.</summary>
        Task DeleteAsync(int blockId, int requestingUserId);
    }
}
