using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);

        /// <summary>false only on a brand-new installation (used to create the first Administrator).</summary>
        Task<bool> AnyUsersAsync();

        /// <summary>CU-001: returns the user if email and password are correct and the user is active; otherwise null.</summary>
        Task<User?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// CU-013 (Administrator only). If no user exists yet, the first one is created as Administrator
        /// and requestingUserId is not needed.
        /// </summary>
        Task<User> CreateAsync(string name, string email, string password, string role, int? requestingUserId);

        /// <summary>CU-013 (Administrator only): changes name, role and whether the user is active. Users are never deleted, only deactivated.</summary>
        Task UpdateAsync(int userId, string name, string role, bool isActive, int requestingUserId);

        Task ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    }
}
