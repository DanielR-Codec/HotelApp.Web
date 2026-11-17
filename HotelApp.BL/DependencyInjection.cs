using HotelApp.BL.Interfaces;
using HotelApp.BL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace HotelApp.BL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBLDependencies(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IGuestService, GuestService>();
            services.AddScoped<IHotelService, HotelService>();
            services.AddScoped<IBlockService, BlockService>();
            services.AddScoped<IFloorService, FloorService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IReservationDetailService, ReservationDetailService>();
            services.AddScoped<IRoomTypeService, RoomTypeService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IReservationAuditService, ReservationAuditService>();
            services.AddScoped<IEmailService, EmailService>();
            services.TryAddSingleton(TimeProvider.System);

            return services;
        }
    }
}
