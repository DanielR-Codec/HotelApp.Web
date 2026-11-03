using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Services
{
    public class TipoHabitacionService : ITipoHabitacionService
    {
        private readonly ITipoHabitacionRepository _repository;

        public TipoHabitacionService(ITipoHabitacionRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TipoHabitacion>> ObtenerTodosAsync() => await _repository.GetAllAsync();
        public async Task<TipoHabitacion?> ObtenerPorIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AgregarAsync(TipoHabitacion tipo) => await _repository.AddAsync(tipo);
        public void Actualizar(TipoHabitacion tipo) => _repository.Update(tipo);
        public void Eliminar(TipoHabitacion tipo) => _repository.Remove(tipo);
    }
}