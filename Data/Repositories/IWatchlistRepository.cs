using AnimeTrackerApi.Models;

namespace AnimeTrackerApi.Data.Repositories
{
    public interface IWatchlistRepository
    {
        Task<WatchlistItem> AddToWatchlistAsync(WatchlistItem item);
        Task<List<WatchlistItem>> GetUserWatchlistAsync(int userId);
        Task<bool> RemoveFromWatchlistAsync(int itemId, int userId);
        Task<bool> UpdateStatusAsync(int itemId, int userId, string newStatus);
        Task<List<WatchlistItem>> GetRecentlyAddedAnimeAsync(int userId, int limit = 5);
        Task<List<WatchlistItem>> GetWatchingAnimeAsync(int userId);
    }
}
