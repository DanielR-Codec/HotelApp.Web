using System.Security.Claims;
using HotelApp.BL.Interfaces;
using HotelApp.DAL.Constants;
using HotelApp.DAL.Exceptions;
using HotelApp.DAL.Models.dbo;
using HotelApp.Web.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.Web.Controllers
{
    /// <summary>CU-001 / RF-011: login and logout. Also the creation of the first Administrator on a new installation.</summary>
    [AllowAnonymous]
    public class AccountController : AppController
    {
        private readonly IUserService _users;

        public AccountController(IUserService users)
        {
            _users = users;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            // Brand-new installation: there is nobody to log in with yet
            var anyUsers = await LoadOrDefaultAsync(() => _users.AnyUsersAsync(), true);
            if (!anyUsers)
                return RedirectToAction(nameof(Setup));

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var user = await _users.AuthenticateAsync(model.Email, model.Password);
                if (user is null)
                {
                    ModelState.AddModelError(string.Empty, "Correo o clave incorrectos, o el usuario está inactivo.");
                    return View(model);
                }

                await SignInAsync(user, model.RememberMe);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }
            catch (HotelAppException ex)
            {
                AddErrors(ex);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Setup()
        {
            var anyUsers = await LoadOrDefaultAsync(() => _users.AnyUsersAsync(), true);
            if (anyUsers)
                return RedirectToAction(nameof(Login));

            return View(new SetupViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Setup(SetupViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // The BL only allows this without a requesting user when there are no users yet
                var user = await _users.CreateAsync(model.Name, model.Email, model.Password, UserRole.Administrator, null);
                await SignInAsync(user, rememberMe: false);

                ShowSuccess($"Bienvenido, {user.Name}. Su cuenta de Administrador fue creada.");
                return RedirectToAction("Index", "Home");
            }
            catch (HotelAppException ex)
            {
                AddErrors(ex);
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        private Task SignInAsync(User user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role)
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties { IsPersistent = rememberMe });
        }
    }
}
