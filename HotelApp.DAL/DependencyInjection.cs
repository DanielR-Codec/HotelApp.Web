using HotelApp.DAL.Base;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Repositories.dbo;
using Microsoft.Extensions.DependencyInjection;

namespace HotelApp.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDALDependencies(this IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGuestRepository, GuestRepository>();
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<IBlockRepository, BlockRepository>();
            services.AddScoped<IFloorRepository, FloorRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IReservationDetailRepository, ReservationDetailRepository>();
            services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
            services.AddScoped<IReservationAuditRepository, ReservationAuditRepository>();

            return services;
        }
    }
}
