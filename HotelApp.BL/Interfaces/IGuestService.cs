using HotelApp.DAL.Models.dbo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelApp.BL.Interfaces
{
    public interface IGuestService
    {
        Task<IEnumerable<Guest>> GetAllAsync();
        Task<Guest?> GetByIdAsync(int id);

        /// <summary>RF-003: searches by document, first name, last name, email or phone.</summary>
        Task<IEnumerable<Guest>> SearchAsync(string text);
        Task<Guest?> GetByDocumentAsync(string document);

        /// <summary>CU-002: registers a new guest (validates legal age, RN-002).</summary>
        Task AddAsync(Guest guest);

        /// <summary>CU-003: corrects a guest's data.</summary>
        Task UpdateAsync(Guest guest);
    }
}
