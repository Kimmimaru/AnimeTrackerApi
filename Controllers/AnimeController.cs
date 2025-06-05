using Microsoft.AspNetCore.Mvc;
using AnimeTrackerApi.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnimeTrackerApi.Models;

namespace AnimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnimeController : ControllerBase
    {
        private readonly JikanService _jikanService;

        public AnimeController(JikanService jikanService)
        {
            _jikanService = jikanService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name parameter is required");

            try
            {
                var results = await _jikanService.AnimeSearch(name);
                return Ok(results);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error calling MyAnimeList API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("recommendations/{animeId}")]
        public async Task<IActionResult> GetRecommendations(int animeId)
        {
            if (animeId <= 0)
                return BadRequest("Valid anime ID is required");

            try
            {
                var results = await _jikanService.GetAnimeRecommendations(animeId);
                return Ok(results);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error calling MyAnimeList API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{animeId}")]
        public async Task<IActionResult> GetAnimeById(int animeId)
        {
            if (animeId <= 0)
                return BadRequest("Valid anime ID is required");

            try
            {
                var result = await _jikanService.GetAnimeById(animeId);
                if (result == null)
                    return NotFound("Anime not found");

                return Ok(result);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error calling MyAnimeList API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{animeId}/reviews")]
        public async Task<IActionResult> GetAnimeReviews(
    int animeId,
    [FromQuery] int page = 1,
    [FromQuery] bool spoilers = false,
    [FromQuery] bool preliminary = true,
    [FromQuery] string sort = "suggested")
        {
            if (animeId <= 0)
                return BadRequest("Valid anime ID is required");

            try
            {
                var results = await _jikanService.GetAnimeReviews(animeId, page, spoilers, preliminary, sort);
                return Ok(results);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error calling MyAnimeList API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterAnime([FromQuery] AnimeFilterParameters filterParams)
        {
            try
            {
                var results = await _jikanService.FilterAnime(filterParams);

                if (!results.Any())
                {
                    return Ok(new List<AnimeSearchResult>());
                }

                return Ok(results);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error calling MyAnimeList API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }
}