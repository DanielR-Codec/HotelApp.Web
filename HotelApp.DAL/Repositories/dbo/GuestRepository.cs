using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelApp.DAL.Base;
using HotelApp.DAL.Context;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Models.dbo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelApp.DAL.Repositories.dbo
{
    public class GuestRepository : BaseRepository<Guest>, IGuestRepository
    {
        private const int MaxSearchResults = 50;
        private const int MaxSearchLength = 100;

        public GuestRepository(HotelContext context, ILogger<GuestRepository> logger) : base(context, logger) { }

        public Task<Guest> GetByDocumentAsync(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                return Task.FromResult<Guest>(null);

            var clean = document.Trim();
            return ExecuteAsync(async () =>
                await _dbSet.FirstOrDefaultAsync(g => g.IdentityDocument == clean),
                nameof(GetByDocumentAsync));
        }

        public Task<IEnumerable<Guest>> SearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Task.FromResult(Enumerable.Empty<Guest>());

            var clean = text.Trim();
            if (clean.Length > MaxSearchLength)
                clean = clean[..MaxSearchLength];

            return ExecuteAsync<IEnumerable<Guest>>(async () =>
                await _dbSet
                    .Where(g => g.IdentityDocument.Contains(clean)
                             || g.FirstName.Contains(clean)
                             || g.LastName.Contains(clean)
                             || g.Email.Contains(clean)
                             || g.Phone.Contains(clean))
                    .OrderBy(g => g.LastName).ThenBy(g => g.FirstName)
                    .Take(MaxSearchResults)
                    .ToListAsync(),
                nameof(SearchAsync));
        }
    }
}
