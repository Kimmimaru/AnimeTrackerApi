using Newtonsoft.Json;

public class AnimeRecommendation
{
    [JsonProperty("recommendation.title")]
    public string Title { get; set; }

    [JsonProperty("recommendation.picture_url")]
    public string PictureUrl { get; set; }

    [JsonProperty("recommendation.myanimelist_url")]
    public string MyAnimeListUrl { get; set; }

    [JsonProperty("recommendation.myanimelist_id")]
    public int MyAnimeListId { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    public string DisplayTitle => $"{Title} {(string.IsNullOrEmpty(Description) ? "" : $"- {Description.Truncate(30)}")}";
}

