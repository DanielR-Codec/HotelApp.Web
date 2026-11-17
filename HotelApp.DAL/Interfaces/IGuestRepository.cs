using System.Collections.Generic;
using System.Threading.Tasks;
using HotelApp.DAL.Models.dbo;

namespace HotelApp.DAL.Interfaces
{
    public interface IGuestRepository : IBaseRepository<Guest>
    {
        Task<Guest> GetByDocumentAsync(string document);

        /// <summary>Searches by document, first name, last name, email or phone (RF-003). Returns at most 50 results.</summary>
        Task<IEnumerable<Guest>> SearchAsync(string text);
    }
}
