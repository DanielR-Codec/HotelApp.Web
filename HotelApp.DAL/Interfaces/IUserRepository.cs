using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> AnyExistsAsync();
    }
}
