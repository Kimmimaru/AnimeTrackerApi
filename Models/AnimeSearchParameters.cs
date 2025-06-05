namespace AnimeTrackerApi.Models
{
    public class AnimeFilterParameters
    {
        public double? MinScore { get; set; }
        public string? Genres { get; set; }
        public int Limit { get; set; } = 10;
    }
}
