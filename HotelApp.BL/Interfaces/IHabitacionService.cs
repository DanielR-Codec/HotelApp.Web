using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    public interface IHabitacionService
    {
        Task<IEnumerable<Habitacion>> ObtenerTodosAsync();
        Task<Habitacion?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Habitacion habitacion);
        void Actualizar(Habitacion habitacion);
        void Eliminar(Habitacion habitacion);
    }
}