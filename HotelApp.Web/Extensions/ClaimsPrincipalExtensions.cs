using System.Security.Claims;
using HotelApp.DAL.Constants;

namespace HotelApp.Web.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>Id of the logged-in user (claim NameIdentifier), or 0 if there is no session.</summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        }

        /// <summary>RF-012: used by the views to hide the configuration options from receptionists.</summary>
        public static bool IsAdministrator(this ClaimsPrincipal user)
        {
            return user.IsInRole(UserRole.Administrator);
        }
    }
}
