using Newtonsoft.Json;

public class AnimeSearchResult
{
    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("picture_url")]
    public string PictureUrl { get; set; }

    [JsonProperty("myanimelist_url")]
    public string MyAnimeListUrl { get; set; }

    [JsonProperty("myanimelist_id")]
    public int MyAnimeListId { get; set; }

}