using System;
using System.Security.Cryptography;
using System.Text;

namespace HotelApp.BL.Helpers
{
    /// <summary>RNF-002: passwords are stored hashed with SHA-256, never in plain text.</summary>
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }

        public static bool Verify(string password, string storedHash)
        {
            return !string.IsNullOrEmpty(storedHash)
                && string.Equals(Hash(password), storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
