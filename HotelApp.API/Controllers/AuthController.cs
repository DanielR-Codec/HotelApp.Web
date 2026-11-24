using HotelApp.API.Dtos;
using HotelApp.API.Infrastructure;
using HotelApp.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelApp.API.Controllers
{
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IUserService _users;
        private readonly JwtTokenService _tokens;

        public AuthController(IUserService users, JwtTokenService tokens)
        {
            _users = users;
            _tokens = tokens;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var user = await _users.AuthenticateAsync(request.Email, request.Password);
            if (user is null)
                return Problem(statusCode: StatusCodes.Status401Unauthorized,
                               title: "No autenticado.",
                               detail: "Correo o clave incorrectos, o el usuario está inactivo.");

            var (token, expiresAtUtc) = _tokens.CreateToken(user);

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAtUtc = expiresAtUtc,
                User = UserInfoDto.FromEntity(user)
            });
        }

        [HttpGet("me")]
        [ProducesResponseType<UserInfoDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserInfoDto>> Me()
        {
            var user = await _users.GetByIdAsync(CurrentUserId);
            if (user is null || !user.IsActive)
                return Problem(statusCode: StatusCodes.Status401Unauthorized,
                               title: "No autenticado.",
                               detail: "El usuario no existe o está inactivo.");

            return Ok(UserInfoDto.FromEntity(user));
        }
    }
}
