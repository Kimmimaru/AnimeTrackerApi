namespace AnimeTrackerApi.Models
{
    public class AnimeRelease
    {
        public string Title { get; set; }
        public string Episode { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string PosterUrl { get; set; }
        public int AnimeId { get; set; }
    }

    public class AnimeInfo
    {
        public int Id { get; set; }
        public string RomajiTitle { get; set; }
        public string EnglishTitle { get; set; }
        public string NativeTitle { get; set; }
        public DateTime PremiereDate { get; set; }
        public string MainTitle { get; set; }
        public string Url { get; set; }
        public List<string> Tags { get; set; }
        public string PosterUrl { get; set; }
        public string Studio { get; set; }
        public string ReleaseDate { get; set; }
        public string Source { get; set; }
        public string Episodes { get; set; }
        public string Synopsis { get; set; }
        public Dictionary<string, string> Links { get; set; }
        public EpisodeInfo EpisodeInfo { get; set; }
    }

    public class EpisodeInfo
    {
        public string EpisodeNumber { get; set; }
        public string Countdown { get; set; }
    }
}