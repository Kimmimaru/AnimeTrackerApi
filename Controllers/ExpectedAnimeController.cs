using AnimeTrackerApi.Models;
using AnimeTrackerApi.Data.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AnimeTrackerApi.Bot.Services;

namespace AnimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpectedAnimeController : ControllerBase
    {
        private readonly IExpectedAnimeRepository _expectedAnimeRepository;

        public ExpectedAnimeController(IExpectedAnimeRepository expectedAnimeRepository)
        {
            _expectedAnimeRepository = expectedAnimeRepository;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToExpected([FromBody] ExpectedAnime anime)
        {
            if (anime.UserId <= 0 || anime.AnimeId <= 0)
                return BadRequest("Invalid user ID or anime ID");

            var result = await _expectedAnimeRepository.AddToExpectedAsync(anime);
            return Ok(result);
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromExpected(
            [FromQuery] int id,
            [FromQuery] int userId)
        {
            if (id <= 0 || userId <= 0)
                return BadRequest("Invalid IDs");

            var result = await _expectedAnimeRepository.RemoveFromExpectedAsync(id, userId);
            return result ? Ok() : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetExpectedAnime([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest("Valid user ID is required");

            var items = await _expectedAnimeRepository.GetUserExpectedAnimeAsync(userId);
            return Ok(items);
        }
    }
}
