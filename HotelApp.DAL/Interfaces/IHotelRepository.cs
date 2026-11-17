using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IHotelRepository : IBaseRepository<Hotel>
    {
        Task<IEnumerable<Hotel>> GetFilteredAsync(bool includeInactive);
        Task<bool> NameExistsAsync(string name, int? excludeHotelId = null);
        Task<bool> HasActiveBlocksAsync(int hotelId);
    }
}
