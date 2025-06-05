using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace AnimeTrackerApi.Models
{
    [Table("expected_anime")]
    public class ExpectedAnime
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

        [Column("poster_url")]
        public string? PosterUrl { get; set; }

        [Column("release_date")]
        public DateTime ReleaseDate { get; set; }

        [Column("added_date")]
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;

        [Column("mal_url")]
        public string? MalUrl { get; set; }
    }
}
