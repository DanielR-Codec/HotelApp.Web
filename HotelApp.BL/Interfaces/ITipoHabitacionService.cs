using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    public interface ITipoHabitacionService
    {
        Task<IEnumerable<TipoHabitacion>> ObtenerTodosAsync();
        Task<TipoHabitacion?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(TipoHabitacion tipo);
        void Actualizar(TipoHabitacion tipo);
        void Eliminar(TipoHabitacion tipo);
    }
}