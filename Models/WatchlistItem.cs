using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeTrackerApi.Models
{
    [Table("watchlist")]
    public class WatchlistItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("anime_id")]
        public int AnimeId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("picture_url")]
        public string? PictureUrl { get; set; }

        [Column("myanimelist_url")]
        public string? MyAnimeListUrl { get; set; }

        [Column("status")]
        public string Status { get; set; } = "PlanToWatch";

        [Column("added_date")]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}