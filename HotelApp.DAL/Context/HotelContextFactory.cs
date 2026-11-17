using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelApp.DAL.Context
{
    /// <summary>
    /// Used only by the EF tools (Add-Migration / Update-Database) to create the context
    /// without a startup project. It points to the same local SQL Server instance as HotelApp.Web/appsettings.json,
    /// so migrations are applied to the same database the application uses.
    /// To use another server, set the HOTELAPP_CONNECTION environment variable or change the string below.
    /// </summary>
    public class HotelContextFactory : IDesignTimeDbContextFactory<HotelContext>
    {
        private const string DefaultConnectionString =
            "Server=localhost;Database=HotelAppDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public HotelContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("HOTELAPP_CONNECTION") ?? DefaultConnectionString;

            var options = new DbContextOptionsBuilder<HotelContext>()
                .UseSqlServer(connectionString)
                .Options;

            return new HotelContext(options);
        }
    }
}
