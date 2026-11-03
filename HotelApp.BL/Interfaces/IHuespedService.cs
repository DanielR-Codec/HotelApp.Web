using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    public interface IHuespedService
    {
        Task<IEnumerable<Huesped>> ObtenerTodosAsync();
        Task<Huesped?> ObtenerPorIdAsync(int id);
        Task AgregarAsync(Huesped huesped);
        void Actualizar(Huesped huesped);
        void Eliminar(Huesped huesped);
    }
}