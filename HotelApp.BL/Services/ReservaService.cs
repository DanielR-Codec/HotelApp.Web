using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Services
{
    public class ReservaService : IReservaService
    {
        private readonly IReservaRepository _repository;

        public ReservaService(IReservaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Reserva>> ObtenerTodosAsync() => await _repository.GetAllAsync();
        public async Task<Reserva?> ObtenerPorIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AgregarAsync(Reserva reserva) => await _repository.AddAsync(reserva);
        public void Actualizar(Reserva reserva) => _repository.Update(reserva);
        public void Eliminar(Reserva reserva) => _repository.Remove(reserva);
    }
}