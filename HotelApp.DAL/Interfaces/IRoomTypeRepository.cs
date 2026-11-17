using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IRoomTypeRepository : IBaseRepository<RoomType>
    {
        /// <summary>true if another room type already uses that name (case-insensitive, whatever the database collation is).</summary>
        Task<bool> NameExistsAsync(string name, int? excludeRoomTypeId = null);
    }
}
