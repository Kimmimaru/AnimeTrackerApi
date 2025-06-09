using AnimeTrackerApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimeTrackerApi.Data.Repositories
{
    public interface IExpectedAnimeRepository
    {
        Task<ExpectedAnime> AddToExpectedAsync(ExpectedAnime anime);
        Task<bool> RemoveFromExpectedAsync(int id, int userId);
        Task<List<ExpectedAnime>> GetUserExpectedAnimeAsync(int userId);
        Task<ExpectedAnime> GetByIdAsync(int id);
        Task<List<ExpectedAnime>> GetAllExpectedAnimeAsync();
        Task<ExpectedAnime> GetByUserAndAnimeIdAsync(int userId, int animeId);
    }
}