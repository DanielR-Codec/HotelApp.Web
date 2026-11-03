using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    public interface IReservaService
    {
        Task<IEnumerable<Reserva>> ObtenerTodosAsync();
        Task<Reserva?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Reserva reserva);
        void Actualizar(Reserva reserva);
        void Eliminar(Reserva reserva);
    }
}