using AnimeTrackerApi.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using AnimeTrackerApi.Models;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using System.Globalization;

namespace AnimeTrackerApi.Services
{
    public class JikanService
    {
        private readonly HttpClient _httpClient;

        public JikanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", "76ef60ff05mshcc3d651cf71e616p1eeab2jsnc90ef0788871");
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", "myanimelist.p.rapidapi.com");
        }

        public async Task<List<AnimeSearchResult>> AnimeSearch(string name)
        {
            try
            {
                var searchRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://myanimelist.p.rapidapi.com/v2/anime/search?q={Uri.EscapeDataString(name)}&n=50"),
                    Headers =
            {
                { "x-rapidapi-key", "76ef60ff05mshcc3d651cf71e616p1eeab2jsnc90ef0788871" },
                { "x-rapidapi-host", "myanimelist.p.rapidapi.com" },
            },
                };

                var searchResponse = await _httpClient.SendAsync(searchRequest);
                searchResponse.EnsureSuccessStatusCode();

                var searchBody = await searchResponse.Content.ReadAsStringAsync();
                var searchResults = JsonConvert.DeserializeObject<List<AnimeSearchResult>>(searchBody);

                return searchResults ?? new List<AnimeSearchResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search error: {ex}");
                return new List<AnimeSearchResult>();
            }
        }



        public async Task<List<AnimeRecommendation>> GetAnimeRecommendations(int animeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://myanimelist.p.rapidapi.com/v2/anime/recommendations/{animeId}");
                response.EnsureSuccessStatusCode();

                var body = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(body);

                var recommendations = jsonResponse["recommendations"]?
                    .Select(r => new AnimeRecommendation
                    {
                        Title = r["recommendation"]?["title"]?.ToString(),
                        PictureUrl = r["recommendation"]?["picture_url"]?.ToString(),
                        MyAnimeListUrl = r["recommendation"]?["myanimelist_url"]?.ToString(),
                        MyAnimeListId = r["recommendation"]?["myanimelist_id"]?.ToObject<int>() ?? 0,
                        Description = r["description"]?.ToString()
                    })
                    .Where(r => !string.IsNullOrEmpty(r.Title) && r.MyAnimeListId > 0)
                    .ToList();

                return recommendations ?? new List<AnimeRecommendation>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting recommendations: {ex}");
                return new List<AnimeRecommendation>();
            }
        }


        public async Task<AnimeDetails> GetAnimeById(int animeId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"anime/{animeId}");
                response.EnsureSuccessStatusCode();

                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        args.ErrorContext.Handled = true;
                        Console.WriteLine($"Error parsing {args.ErrorContext.Path}: {args.ErrorContext.Error.Message}");
                    },
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<AnimeDetails>(body, settings);

                if (result == null)
                    throw new Exception("Не вдалося обробити відповідь API");

                result.MyAnimeListId = animeId;
                result.MyAnimeListUrl ??= $"https://myanimelist.net/anime/{animeId}";

                return result;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("Це аніме дуже нове або ще не вийшло, тому його немає в нашій базі");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting anime {animeId}: {ex}");
                throw;
            }
        }

        public async Task<AnimeReviewsResponse> GetAnimeReviews(int animeId, int page = 1, bool includeSpoilers = false, bool includePreliminary = true, string sort = "suggested")
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://myanimelist.p.rapidapi.com/v2/anime/reviews/{animeId}?p={page}&spoilers={includeSpoilers.ToString().ToLower()}&preliminary={includePreliminary.ToString().ToLower()}&sort={sort}"),
                    Headers =
            {
                { "x-rapidapi-key", "76ef60ff05mshcc3d651cf71e616p1eeab2jsnc90ef0788871" },
                { "x-rapidapi-host", "myanimelist.p.rapidapi.com" },
            }
                };

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"API request failed: {response.StatusCode}\n{content}");
                }

                var result = JsonConvert.DeserializeObject<AnimeReviewsResponse>(content);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FULL ERROR DETAILS: {ex}");
                throw;
            }
        }

        public async Task<List<AnimeSearchResult>> FilterAnime(AnimeFilterParameters filterParams)
        {
            var keywords = new[]
            {
            "girl", "boy", "love", "school", "action",
            "fantasy", "adventure", "comedy", "drama", "mystery"
        };

            var allResults = new List<AnimeSearchResult>();
            var rnd = new Random();
            var delayMs = 1100;

            foreach (var kw in keywords)
            {
                try
                {
                    var qp = new Dictionary<string, string?>
                    {
                        ["q"] = kw,
                        ["limit"] = filterParams.Limit.ToString(),
                        ["min_score"] = filterParams.MinScore?.ToString(CultureInfo.InvariantCulture),
                        ["genres"] = filterParams.Genres
                    }
                        .Where(x => !string.IsNullOrEmpty(x.Value))
                        .ToDictionary(x => x.Key, x => x.Value!);

                    var url = QueryHelpers.AddQueryString(
                        "https://myanimelist.p.rapidapi.com/v2/anime/search",
                        qp
                    );

                    using var req = new HttpRequestMessage(HttpMethod.Get, url);
                    req.Headers.TryAddWithoutValidation("x-rapidapi-key",
                        "165d058607msh8ab3046a0d8716bp1fa923jsnec7d4e350aa5");
                    req.Headers.TryAddWithoutValidation("x-rapidapi-host", "myanimelist.p.rapidapi.com");

                    var resp = await _httpClient.SendAsync(req);
                    resp.EnsureSuccessStatusCode();

                    var body = await resp.Content.ReadAsStringAsync();
                    List<AnimeSearchResult> results;

                    if (body.TrimStart().StartsWith("["))
                    {
                        results = JsonConvert
                                      .DeserializeObject<List<AnimeSearchResult>>(body)
                                  ?? new List<AnimeSearchResult>();
                    }
                    else
                    {
                        var jo = JObject.Parse(body);
                        var arr = jo["data"] as JArray;
                        results = arr == null
                            ? new List<AnimeSearchResult>()
                            : arr
                                .Select(x => x["node"]?.ToObject<AnimeSearchResult>())
                                .Where(x => x != null)!
                                .Cast<AnimeSearchResult>()
                                .ToList();
                    }

                    allResults.AddRange(results);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error filtering '{kw}': {ex.Message}");
                }

                await Task.Delay(delayMs);
            }

            var unique = allResults
                .GroupBy(a => a.MyAnimeListId)
                .Select(g => g.First())
                .ToList();

            for (var i = unique.Count - 1; i > 0; i--)
            {
                var j = rnd.Next(i + 1);
                (unique[i], unique[j]) = (unique[j], unique[i]);
            }

            return unique;
        }


    }
}