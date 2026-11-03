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
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IHuespedRepository, HuespedRepository>();
            services.AddScoped<IHabitacionRepository, HabitacionRepository>();
            services.AddScoped<IReservaRepository, ReservaRepository>();
            services.AddScoped<IDetalleReservaRepository, DetalleReservaRepository>();
            services.AddScoped<ITipoHabitacionRepository, TipoHabitacionRepository>();

            return services;
        }
    }
}