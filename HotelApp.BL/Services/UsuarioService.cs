using HotelApp.BL.Interfaces;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Usuario>> ObtenerTodosAsync() => await _repository.GetAllAsync();
        public async Task<Usuario?> ObtenerPorIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task AgregarAsync(Usuario usuario) => await _repository.AddAsync(usuario);
        public void Actualizar(Usuario usuario) => _repository.Update(usuario);
        public void Eliminar(Usuario usuario) => _repository.Remove(usuario);
    }
}