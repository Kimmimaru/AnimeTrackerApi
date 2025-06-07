using AnimeTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeTrackerApi.Data.Repositories
{
    public class WatchlistRepository : IWatchlistRepository
    {
        private readonly AppDbContext _context;

        public WatchlistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WatchlistItem> AddToWatchlistAsync(WatchlistItem item)
        {
            var existingItem = await _context.WatchlistItems
                .FirstOrDefaultAsync(x => x.UserId == item.UserId && x.AnimeId == item.AnimeId);

            if (existingItem != null)
            {
                return existingItem;
            }

            _context.WatchlistItems.Add(item);

            try
            {
                await _context.SaveChangesAsync();
                return item;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error: {ex.InnerException?.Message ?? ex.Message}");
                throw;
            }
        }

        public async Task<List<WatchlistItem>> GetUserWatchlistAsync(int userId)
        {

            return await _context.WatchlistItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AddedDate)
                .Select(x => new WatchlistItem
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    AnimeId = x.AnimeId,
                    Title = x.Title,
                    Description = x.Description ?? string.Empty,
                    PictureUrl = x.PictureUrl ?? string.Empty,
                    MyAnimeListUrl = x.MyAnimeListUrl ?? "https://myanimelist.net",
                    Status = x.Status,
                    AddedDate = x.AddedDate
                })
                .ToListAsync();
        }

        public async Task<bool> RemoveFromWatchlistAsync(int itemId, int userId)
        {
            var item = await _context.WatchlistItems
                .FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId);

            if (item != null)
            {
                _context.WatchlistItems.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> UpdateStatusAsync(int itemId, int userId, string newStatus)
        {
            var item = await _context.WatchlistItems
                .FirstOrDefaultAsync(x => x.Id == itemId && x.UserId == userId);

            if (item != null)
            {
                item.Status = newStatus;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<List<WatchlistItem>> GetRecentlyAddedAnimeAsync(int userId, int limit = 5)
        {
            return await _context.WatchlistItems
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.AddedDate)
                .Take(limit)
                .Select(x => new WatchlistItem
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    AnimeId = x.AnimeId,
                    Title = x.Title,
                    Description = x.Description,
                    PictureUrl = x.PictureUrl,
                    MyAnimeListUrl = x.MyAnimeListUrl,
                    Status = x.Status,
                    AddedDate = x.AddedDate
                })
                .ToListAsync();
        }

        public async Task<List<WatchlistItem>> GetWatchingAnimeAsync(int userId)
        {
            return await _context.WatchlistItems
                .Where(x => x.UserId == userId && x.Status == "Watching")
                .OrderByDescending(x => x.AddedDate)
                .Select(x => new WatchlistItem
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    AnimeId = x.AnimeId,
                    Title = x.Title,
                    Description = x.Description,
                    PictureUrl = x.PictureUrl,
                    MyAnimeListUrl = x.MyAnimeListUrl,
                    Status = x.Status,
                    AddedDate = x.AddedDate
                })
                .ToListAsync();
        }

        public async Task<bool> CheckAnimeInWatchlistAsync(int userId, int animeId)
        {
            return await _context.WatchlistItems
                .AnyAsync(x => x.UserId == userId && x.AnimeId == animeId);
        }


    }
}