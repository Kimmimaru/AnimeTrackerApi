﻿using AnimeTrackerApi.Models;
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

            var existing = await _expectedAnimeRepository.GetByUserAndAnimeIdAsync(anime.UserId, anime.AnimeId);
            if (existing != null)
            {
                return Ok(existing);
            }

            anime.AddedDate = DateTime.UtcNow;

            if (anime.ReleaseDate == default)
            {
                anime.ReleaseDate = DateTime.UtcNow.Date;
            }

            if (anime.ReleaseDate.Kind == DateTimeKind.Unspecified)
            {
                anime.ReleaseDate = DateTime.SpecifyKind(anime.ReleaseDate, DateTimeKind.Utc);
            }

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

        [HttpGet("released-today")]
        public async Task<IActionResult> GetReleasedToday()
        {
            var today = DateTime.UtcNow.Date;
            var items = await _expectedAnimeRepository.GetAllExpectedAnimeAsync();

            var releasedToday = items
                .Where(x => x.ReleaseDate.Date == today && x.AddedDate.Date < today) 
                .ToList();

            return Ok(releasedToday);
        }
    }
}
