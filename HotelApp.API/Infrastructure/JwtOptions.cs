namespace HotelApp.API.Infrastructure
{
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "HotelApp.API";
        public string Audience { get; set; } = "HotelApp.Clients";

        public string Key { get; set; } = string.Empty;

        public int ExpirationMinutes { get; set; } = 480;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Key) || Key.Length < 32)
                throw new InvalidOperationException("Jwt:Key must have at least 32 characters.");

            if (ExpirationMinutes <= 0)
                throw new InvalidOperationException("Jwt:ExpirationMinutes must be greater than zero.");
        }
    }
}
