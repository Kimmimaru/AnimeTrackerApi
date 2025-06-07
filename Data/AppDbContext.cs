using AnimeTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeTrackerApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<WatchlistItem> WatchlistItems { get; set; }

        public DbSet<ExpectedAnime> ExpectedAnime { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WatchlistItem>(entity =>
            {
                entity.ToTable("watchlist");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AnimeId).HasColumnName("anime_id");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.PictureUrl).HasColumnName("picture_url");
                entity.Property(e => e.MyAnimeListUrl).HasColumnName("myanimelist_url");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.AddedDate).HasColumnName("added_date");

                entity.HasIndex(w => new { w.UserId, w.AnimeId }).IsUnique();
            });

            modelBuilder.Entity<ExpectedAnime>(entity =>
            {
                entity.ToTable("expected_anime");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AnimeId).HasColumnName("anime_id");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.PosterUrl).HasColumnName("poster_url");
                entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
                entity.Property(e => e.AddedDate).HasColumnName("added_date");
                entity.Property(e => e.MalUrl).HasColumnName("mal_url");

                entity.HasIndex(x => new { x.UserId, x.AnimeId }).IsUnique();
            });
        }


    }
}
