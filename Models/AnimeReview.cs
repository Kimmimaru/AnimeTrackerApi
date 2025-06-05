using Newtonsoft.Json;
using System.Collections.Generic;

namespace AnimeTrackerApi.Models
{
    public class AnimeReview
    {
        [JsonProperty("user")]
        public ReviewUser User { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("text")]
        public ReviewText Text { get; set; }

        [JsonProperty("date")]
        public ReviewDate Date { get; set; }

        [JsonProperty("object")]
        public ReviewObject Object { get; set; }
    }

    public class ReviewUser
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("picture_url")]
        public string PictureUrl { get; set; }
    }

    public class ReviewText
    {
        [JsonProperty("visible")]
        public string Visible { get; set; }

        [JsonProperty("hidden")]
        public string Hidden { get; set; }

        [JsonProperty("full")]
        public string Full { get; set; }
    }

    public class ReviewDate
    {
        [JsonProperty("date_str")]
        public string DateStr { get; set; }

        [JsonProperty("time_str")]
        public string TimeStr { get; set; }

        [JsonProperty("timestamp")]
        public double Timestamp { get; set; }
    }

    public class ReviewObject
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("mal_url")]
        public string MalUrl { get; set; }

        [JsonProperty("mal_id")]
        public int MalId { get; set; }

        [JsonProperty("all_reviews_url")]
        public string AllReviewsUrl { get; set; }

        [JsonProperty("picture_url")]
        public string PictureUrl { get; set; }
    }

    public class AnimeReviewsResponse
    {
        [JsonProperty("reviews")]
        public List<AnimeReview> Reviews { get; set; }

        [JsonProperty("paging")]
        public PagingInfo Paging { get; set; }
    }

    public class PagingInfo
    {
        [JsonProperty("next")]
        public string NextPageUrl { get; set; }
    }
}