using HotelApp.DAL.Exceptions;
using HotelApp.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.Web.Controllers
{
    /// <summary>
    /// Base of the application controllers: current user and the way errors are shown.
    /// Every controlled error (DAL or BL) is a HotelAppException; its message is shown to the user as-is.
    /// </summary>
    public abstract class AppController : Controller
    {
        private const string SuccessKey = "Success";
        private const string ErrorKey = "Error";

        protected int CurrentUserId => User.GetUserId();

        /// <summary>Message shown after a redirect (Post-Redirect-Get).</summary>
        protected void ShowSuccess(string message) => TempData[SuccessKey] = message;

        /// <summary>Message shown after a redirect (Post-Redirect-Get).</summary>
        protected void ShowError(string message) => TempData[ErrorKey] = message;

        /// <summary>Message shown on the view rendered by the current request.</summary>
        protected void ShowErrorNow(string message) => ViewData[ErrorKey] = message;

        /// <summary>Adds the error to the form so it is shown in the validation summary.</summary>
        protected void AddErrors(HotelAppException ex)
        {
            if (ex is EntityValidationException validation)
            {
                foreach (var error in validation.Errors)
                    ModelState.AddModelError(string.Empty, error);
            }
            else
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        /// <summary>Loads data for a view; if the database fails, the view is shown anyway with the fallback value and an error message.</summary>
        protected async Task<T> LoadOrDefaultAsync<T>(Func<Task<T>> load, T fallback)
        {
            try
            {
                return await load();
            }
            catch (HotelAppException ex)
            {
                ShowErrorNow(ex.Message);
                return fallback;
            }
        }
    }
}
