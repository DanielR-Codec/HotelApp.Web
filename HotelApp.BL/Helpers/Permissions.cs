using System;
using System.Threading.Tasks;
using HotelApp.BL.Constants;
using HotelApp.BL.Exceptions;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.BL.Helpers
{
    public static class Permissions
    {
        public static bool IsAdministrator(string role)
        {
            return string.Equals(role, UserRole.Administrator, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsReceptionist(string role)
        {
            return string.Equals(role, UserRole.Receptionist, StringComparison.OrdinalIgnoreCase);
        }

        public static bool CanModifyReservation(string role, string reservationStatus)
        {
            if (IsAdministrator(role))
            {
                return true;
            }

            if (IsReceptionist(role))
            {
                return reservationStatus != ReservationStatus.Cancelled && reservationStatus != ReservationStatus.Completed;
            }

            return false;
        }

        /// <summary>Returns the user if it exists and is active; otherwise throws a BusinessRuleException.</summary>
        public static async Task<User> GetActiveUserAsync(IUserRepository users, int userId)
        {
            var user = await users.GetByIdAsync(userId);

            if (user is null || !user.IsActive)
                throw new BusinessRuleException(ErrorCodes.Auth, "El usuario no existe o está inactivo.");

            return user;
        }

        /// <summary>Requires the user to be active and an Administrator. The code lets you point to the rule (for example "RN-014").</summary>
        public static async Task<User> RequireAdministratorAsync(IUserRepository users, int userId, string code = ErrorCodes.Forbidden)
        {
            var user = await GetActiveUserAsync(users, userId);

            if (!IsAdministrator(user.Role))
                throw new BusinessRuleException(code, "Esta operación solo la puede realizar un Administrador.");

            return user;
        }
    }
}
