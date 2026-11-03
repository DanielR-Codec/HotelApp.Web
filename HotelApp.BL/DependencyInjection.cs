using HotelApp.BL.Interfaces;
using HotelApp.BL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HotelApp.BL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBLDependencies(this IServiceCollection services)
        {
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IHuespedService, HuespedService>();
            services.AddScoped<IHabitacionService, HabitacionService>();
            services.AddScoped<IReservaService, ReservaService>();
            services.AddScoped<IDetalleReservaService, DetalleReservaService>();
            services.AddScoped<ITipoHabitacionService, TipoHabitacionService>();

            return services;
        }
    }
}