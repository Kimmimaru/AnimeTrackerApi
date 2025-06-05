using AnimeTrackerApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnimeTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReleasesController : ControllerBase
    {
        private readonly LiveChartService _liveChartService;

        public ReleasesController(LiveChartService liveChartService)
        {
            _liveChartService = liveChartService;
        }

        [HttpGet("schedule")]
        public async Task<IActionResult> GetUpcomingReleases()
        {
            var releases = await _liveChartService.GetUpcomingReleasesAsync();
            return Ok(releases);
        }

        [HttpGet("mainpage")]
        public async Task<IActionResult> GetMainPageReleases()
        {
            try
            {
                var animeList = await _liveChartService.GetMainPageAnimeAsync();
                return Ok(animeList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}