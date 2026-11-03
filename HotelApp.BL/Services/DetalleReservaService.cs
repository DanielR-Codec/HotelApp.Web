using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Services
{
    public class DetalleReservaService : IDetalleReservaService
    {
        private readonly IDetalleReservaRepository _repository;

        public DetalleReservaService(IDetalleReservaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DetalleReserva>> ObtenerTodosAsync() => await _repository.GetAllAsync();
        public async Task<DetalleReserva?> ObtenerPorIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AgregarAsync(DetalleReserva detalle) => await _repository.AddAsync(detalle);
        public void Actualizar(DetalleReserva detalle) => _repository.Update(detalle);
        public void Eliminar(DetalleReserva detalle) => _repository.Remove(detalle);
    }
}