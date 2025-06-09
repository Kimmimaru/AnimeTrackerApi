using AnimeTrackerApi.Data.Repositories;
using AnimeTrackerApi.Data;
using AnimeTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeTrackerApi.Models
{
    public class ExpectedAnimeRepository : IExpectedAnimeRepository
    {
        private readonly AppDbContext _context;

        public ExpectedAnimeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ExpectedAnime> AddToExpectedAsync(ExpectedAnime anime)
        {
            var existing = await _context.ExpectedAnime
                .FirstOrDefaultAsync(x => x.UserId == anime.UserId && x.AnimeId == anime.AnimeId);

            if (existing != null)
            {
                return existing;
            }

            if (anime.ReleaseDate.Kind == DateTimeKind.Unspecified)
            {
                anime.ReleaseDate = DateTime.SpecifyKind(anime.ReleaseDate, DateTimeKind.Utc);
            }

            _context.ExpectedAnime.Add(anime);
            await _context.SaveChangesAsync();
            return anime;
        }



        public async Task<List<ExpectedAnime>> GetUserExpectedAnimeAsync(int userId)
        {
            return await _context.ExpectedAnime
                .Where(x => x.UserId == userId)
                .OrderBy(x => x.ReleaseDate)
                .ToListAsync();
        }

        public async Task<ExpectedAnime> GetByIdAsync(int id)
        {
            return await _context.ExpectedAnime.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> RemoveFromExpectedAsync(int id, int userId)
        {
            var item = await _context.ExpectedAnime
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (item == null) return false;

            _context.ExpectedAnime.Remove(item);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ExpectedAnime>> GetAllExpectedAnimeAsync()
        {
            return await _context.ExpectedAnime
                .Where(x => x.ReleaseDate >= DateTime.UtcNow.Date)
                .OrderBy(x => x.ReleaseDate)
                .ToListAsync();
        }
    }
}
