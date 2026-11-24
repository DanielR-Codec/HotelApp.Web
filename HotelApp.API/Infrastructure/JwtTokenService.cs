using System.Security.Claims;
using System.Text;
using HotelApp.DAL.Models.dbo;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HotelApp.API.Infrastructure
{
    /// <summary>Issues the JWT that identifies the logged-in user (same claims the MVC site keeps in its cookie).</summary>
    public class JwtTokenService
    {
        private readonly JwtOptions _options;
        private readonly TimeProvider _timeProvider;

        public JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider)
        {
            _options = options.Value;
            _timeProvider = timeProvider;
        }

        public (string Token, DateTime ExpiresAtUtc) CreateToken(User user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var expires = now.AddMinutes(_options.ExpirationMinutes);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                IssuedAt = now,
                NotBefore = now,
                Expires = expires,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                    new Claim(ClaimTypes.Role, user.Role ?? string.Empty)
                }),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                    SecurityAlgorithms.HmacSha256)
            };

            return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
        }
    }
}
