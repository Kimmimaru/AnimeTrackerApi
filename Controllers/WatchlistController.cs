using AnimeTrackerApi.Models;
using AnimeTrackerApi.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using AnimeTrackerApi.Services;

namespace AnimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WatchlistController : ControllerBase
    {
        private readonly IWatchlistRepository _watchlistRepository;
        private readonly JikanService _jikanService;

        public WatchlistController(IWatchlistRepository watchlistRepository, JikanService jikanService)
        {
            _watchlistRepository = watchlistRepository;
            _jikanService = jikanService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToWatchlist([FromQuery] int userId, [FromQuery] int animeId, [FromQuery] string status = "PlanToWatch")
        {
            if (animeId <= 0 || userId <= 0)
                return BadRequest("Valid anime ID and user ID are required");

            var animeDetails = await _jikanService.GetAnimeById(animeId);
            if (animeDetails == null)
                return NotFound("Anime not found");

            var title = !string.IsNullOrEmpty(animeDetails.EnglishTitle)
                ? animeDetails.EnglishTitle
                : !string.IsNullOrEmpty(animeDetails.OriginalTitle)
                    ? animeDetails.OriginalTitle
                    : "Unknown Title";

            var item = new WatchlistItem
            {
                UserId = userId,
                AnimeId = animeId,
                Title = animeDetails.GetBestAvailableTitle(),
                Description = animeDetails.Synopsis,
                PictureUrl = animeDetails.PictureUrl,
                MyAnimeListUrl = $"https://localhost:7115/anime/{animeId}",
                Status = status
            };

            var result = await _watchlistRepository.AddToWatchlistAsync(item);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetWatchlist([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest("Valid user ID is required");

            var items = await _watchlistRepository.GetUserWatchlistAsync(userId);
            return Ok(items);
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveFromWatchlist(
            [FromQuery] int itemId,
            [FromQuery] int userId)
        {
            if (itemId <= 0 || userId <= 0)
                return BadRequest("Valid IDs are required");

            var result = await _watchlistRepository.RemoveFromWatchlistAsync(itemId, userId);
            return result ? Ok() : NotFound();
        }

        [HttpPatch("update-status")]
        public async Task<IActionResult> UpdateStatus(
            [FromQuery] int itemId,
            [FromQuery] int userId,
            [FromQuery] string newStatus)
        {
            if (itemId <= 0 || userId <= 0)
                return BadRequest("Valid IDs are required");

            var result = await _watchlistRepository.UpdateStatusAsync(itemId, userId, newStatus);
            return result ? Ok() : NotFound();
        }

        [HttpGet("recently-added")]
        public async Task<IActionResult> GetRecentlyAddedAnime(
    [FromQuery] int userId,
    [FromQuery] int limit = 5)
        {
            if (userId <= 0)
                return BadRequest("Valid user ID is required");

            try
            {
                var items = await _watchlistRepository.GetRecentlyAddedAnimeAsync(userId, limit);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("watching")]
        public async Task<IActionResult> GetWatchingAnime([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest("Valid user ID is required");

            try
            {
                var items = await _watchlistRepository.GetWatchingAnimeAsync(userId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        
    }
}


