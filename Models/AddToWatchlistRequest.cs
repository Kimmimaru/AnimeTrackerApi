using System.ComponentModel.DataAnnotations;

namespace AnimeTrackerApi.Models
{
    public class AddToWatchlistRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int AnimeId { get; set; }

        public string Status { get; set; } = "PlanToWatch";
    }
}

