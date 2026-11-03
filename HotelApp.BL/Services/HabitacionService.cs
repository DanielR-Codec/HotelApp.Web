using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Services
{
    public class HabitacionService : IHabitacionService
    {
        private readonly IHabitacionRepository _repository;

        public HabitacionService(IHabitacionRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Habitacion>> ObtenerTodosAsync() => await _repository.GetAllAsync();
        public async Task<Habitacion?> ObtenerPorIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AgregarAsync(Habitacion habitacion) => await _repository.AddAsync(habitacion);
        public void Actualizar(Habitacion habitacion) => _repository.Update(habitacion);
        public void Eliminar(Habitacion habitacion) => _repository.Remove(habitacion);
    }
}