using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Services
{
    public class HuespedService : IHuespedService
    {
        private readonly IHuespedRepository _repository;

        public HuespedService(IHuespedRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Huesped>> ObtenerTodosAsync() => await _repository.GetAllAsync();
        public async Task<Huesped?> ObtenerPorIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AgregarAsync(Huesped huesped) => await _repository.AddAsync(huesped);
        public void Actualizar(Huesped huesped) => _repository.Update(huesped);
        public void Eliminar(Huesped huesped) => _repository.Remove(huesped);
    }
}