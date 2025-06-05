using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace AnimeTrackerApi.Models
{
    public class AnimeDetails
    {
        [JsonProperty("title_ov")]
        public string OriginalTitle { get; set; }

        [JsonProperty("title_en")]
        public string EnglishTitle { get; set; }

        [JsonProperty("title")]
        public string DefaultTitle { get; set; }

        public string GetBestAvailableTitle()
        {
            return !string.IsNullOrEmpty(EnglishTitle)
                ? EnglishTitle
                : !string.IsNullOrEmpty(OriginalTitle)
                    ? OriginalTitle
                    : !string.IsNullOrEmpty(DefaultTitle)
                        ? DefaultTitle
                        : "Unknown Title";
        }


        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("alternative_titles")]
        public AlternativeTitles AlternativeTitles { get; set; }

        [JsonProperty("information")]
        public AnimeInformation Information { get; set; }

        [JsonProperty("statistics")]
        public AnimeStatistics Statistics { get; set; }

        [JsonProperty("picture_url")]
        public string PictureUrl { get; set; }

        public int MyAnimeListId { get; set; }
        public string MyAnimeListUrl { get; set; }
    }

    public class AlternativeTitles
    {
        [JsonProperty("synonyms")]
        public string Synonyms { get; set; }

        [JsonProperty("japanese")]
        public string Japanese { get; set; }

        [JsonProperty("english")]
        public string English { get; set; }

        [JsonProperty("german")]
        public string German { get; set; }

        [JsonProperty("spanish")]
        public string Spanish { get; set; }

        [JsonProperty("french")]
        public string French { get; set; }
    }

    public class AnimeInformation
    {
        [JsonProperty("type")]
        public List<AnimeType> Type { get; set; }

        [JsonProperty("episodes")]
        public string Episodes { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("aired")]
        public string Aired { get; set; }

        [JsonProperty("premiered")]
        public PremieredList Premiered { get; set; } = new PremieredList();

        [JsonProperty("broadcast")]
        public string Broadcast { get; set; }

        [JsonProperty("producers")]
        public List<AnimeProducer> Producers { get; set; }

        [JsonProperty("licensors")]
        public List<AnimeLicensor> Licensors { get; set; }

        [JsonProperty("studios")]
        public List<AnimeStudio> Studios { get; set; }

        [JsonProperty("source")]
        public object Source { get; set; }

        public string GetSourceString()
        {
            if (Source is string str)
                return str;

            if (Source is JArray array && array.Count > 0)
                return array[0]?["name"]?.ToString() ?? "Unknown";

            return "Unknown";
        }


        [JsonProperty("genres")]
        public List<AnimeGenre> Genres { get; set; }

        [JsonProperty("demographic")]
        public List<AnimeDemographic> Demographic { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }
    }

    public class AnimeStatistics
    {
        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("ranked")]
        public int Ranked { get; set; }

        [JsonProperty("popularity")]
        public int Popularity { get; set; }

        [JsonProperty("members")]
        public int Members { get; set; }

        [JsonProperty("favorites")]
        public int Favorites { get; set; }
    }

    public class PremieredConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<AnimeSeason>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                if (reader.TokenType == JsonToken.String && reader.Value?.ToString() == "None")
                {
                    return new List<AnimeSeason>();
                }

                if (reader.TokenType == JsonToken.StartObject)
                {
                    var season = serializer.Deserialize<AnimeSeason>(reader);
                    return new List<AnimeSeason> { season };
                }

                if (reader.TokenType == JsonToken.StartArray)
                {
                    return serializer.Deserialize<List<AnimeSeason>>(reader);
                }

                return new List<AnimeSeason>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing premiered data: {ex.Message}");
                return new List<AnimeSeason>();
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class PremieredListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PremieredList);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new PremieredList();

            if (reader.TokenType == JsonToken.String && reader.Value?.ToString() == "None")
            {
                return result;
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                var singleItem = serializer.Deserialize<PremieredData>(reader);
                if (singleItem != null)
                {
                    result.Add(singleItem);
                }
                return result;
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray array = JArray.Load(reader);
                foreach (var item in array)
                {
                    if (item.Type == JTokenType.Object)
                    {
                        var seasonData = item.ToObject<PremieredData>();
                        if (seasonData != null)
                        {
                            result.Add(seasonData);
                        }
                    }
                }
                return result;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }

    public class AnimeType { public string Name { get; set; } public string Url { get; set; } }
    public class AnimeSeason
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("season")]
        public string Season { get; set; }

        [JsonProperty("year")]
        public int? Year { get; set; }
    }

    public class PremieredData
    {
        [JsonProperty("season")]
        public string Season { get; set; }

        [JsonProperty("year")]
        public int? Year { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    [JsonConverter(typeof(PremieredListConverter))]
    public class PremieredList : List<PremieredData> { }

    public class AnimeProducer { public string Name { get; set; } public string Url { get; set; } }
    public class AnimeLicensor { public string Name { get; set; } public string Url { get; set; } }
    public class AnimeStudio { public string Name { get; set; } public string Url { get; set; } }
    public class AnimeSource { public string Name { get; set; } public string Url { get; set; } }
    public class AnimeGenre { public string Name { get; set; } public string Url { get; set; } }
    public class AnimeDemographic { public string Name { get; set; } public string Url { get; set; } }
}