using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    public interface IDetalleReservaService
    {
        Task<IEnumerable<DetalleReserva>> ObtenerTodosAsync();
        Task<DetalleReserva?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(DetalleReserva detalle);
        void Actualizar(DetalleReserva detalle);
        void Eliminar(DetalleReserva detalle);
    }
}