using AnimeTrackerApi.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json.Linq;
using AnimeTrackerApi.Models;
using System.Net;

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
            var baseKeywords = new List<string>
    {
        "girl", "boy", "love", "school", "action",
        "fantasy", "adventure", "comedy", "drama", "mystery"
    };

            var allResults = new List<AnimeSearchResult>();
            var delayBetweenRequests = TimeSpan.FromMilliseconds(1100);
            var random = new Random();

            foreach (var keyword in baseKeywords)
            {
                try
                {
                    var url = $"https://myanimelist.p.rapidapi.com/v2/anime/search?q={Uri.EscapeDataString(keyword)}&n={filterParams.Limit}";

                    if (filterParams.MinScore.HasValue)
                    {
                        url += $"&score={filterParams.MinScore}";
                    }

                    if (!string.IsNullOrEmpty(filterParams.Genres))
                    {
                        url += $"&genre={filterParams.Genres}";
                    }

                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(url),
                        Headers =
                {
                    { "x-rapidapi-key", "76ef60ff05mshcc3d651cf71e616p1eeab2jsnc90ef0788871" },
                    { "x-rapidapi-host", "myanimelist.p.rapidapi.com" },
                },
                    };

                    var response = await _httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    var body = await response.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<List<AnimeSearchResult>>(body);

                    if (results != null && results.Any())
                    {
                        allResults.AddRange(results);
                    }

                    if (keyword != baseKeywords.Last())
                    {
                        await Task.Delay(delayBetweenRequests);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error filtering with keyword '{keyword}': {ex.Message}");
                }
            }

            var uniqueResults = allResults
                .GroupBy(a => a.MyAnimeListId)
                .Select(g => g.First())
                .ToList();

            for (int i = uniqueResults.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = uniqueResults[i];
                uniqueResults[i] = uniqueResults[j];
                uniqueResults[j] = temp;
            }

            return uniqueResults;
        }


    }
}