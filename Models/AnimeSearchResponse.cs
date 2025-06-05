using Newtonsoft.Json;

namespace AnimeTrackerApi.Models
{
    public class AnimeSearchResponse
    {
        [JsonProperty("data")]
        public List<AnimeSearchResult> Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        [JsonProperty("last_visible_page")]
        public int LastVisiblePage { get; set; }

        [JsonProperty("has_next_page")]
        public bool HasNextPage { get; set; }
    }
}
