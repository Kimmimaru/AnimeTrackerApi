using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using AnimeTrackerApi.Models;

namespace AnimeTrackerApi.Services
{
    public class LiveChartService
    {
        private readonly HttpClient _httpClient;

        public LiveChartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
        }

        public async Task<List<AnimeRelease>> GetUpcomingReleasesAsync()
        {
            var html = await _httpClient.GetStringAsync("https://www.livechart.me/schedule/tv");
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var releases = new List<AnimeRelease>();

            var animeNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'lc-timetable-anime-block')]");

            if (animeNodes == null) return releases;

            foreach (var node in animeNodes)
            {
                try
                {
                    var title = node.GetAttributeValue("data-schedule-anime-title", "");
                    var animeId = node.GetAttributeValue("data-schedule-anime-id", 0);
                    var unixTimestamp = node.GetAttributeValue("data-schedule-anime-release-date-value", 0L);

                    var episodeNode = node.SelectSingleNode(".//span[contains(@class, 'font-medium')]");
                    var episode = episodeNode?.InnerText.Trim() ?? "Unknown";

                    var posterNode = node.SelectSingleNode(".//img[contains(@class, 'lc-tt-poster')]");
                    var posterUrl = posterNode?.GetAttributeValue("src", "");

                    releases.Add(new AnimeRelease
                    {
                        Title = title,
                        Episode = episode,
                        ReleaseDate = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime,
                        PosterUrl = posterUrl,
                        AnimeId = animeId
                    });
                }
                catch 
                {  
                }
            }

            return releases;
        }

        public async Task<List<AnimeInfo>> GetMainPageAnimeAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://www.livechart.me/");
                response.EnsureSuccessStatusCode();

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var animeList = new List<AnimeInfo>();

                var animeNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'anime')]");

                if (animeNodes == null) return animeList;

                foreach (var node in animeNodes)
                {
                    try
                    {
                        var anime = new AnimeInfo
                        {
                            Id = node.GetAttributeValue("data-anime-id", 0),
                            RomajiTitle = node.GetAttributeValue("data-romaji", ""),
                            EnglishTitle = node.GetAttributeValue("data-english", ""),
                            NativeTitle = node.GetAttributeValue("data-native", ""),
                            PremiereDate = DateTimeOffset.FromUnixTimeSeconds(
                                node.GetAttributeValue("data-premiere", 0L)).DateTime,

                            MainTitle = node.SelectSingleNode(".//h3[@class='main-title']/a")?.InnerText.Trim(),
                            Url = "https://www.livechart.me" + node.SelectSingleNode(".//h3[@class='main-title']/a")?
                                .GetAttributeValue("href", ""),

                            Tags = node.SelectNodes(".//ol[@class='anime-tags']/li/a")?
                                .Select(t => t.InnerText.Trim()).ToList(),

                            PosterUrl = GetPosterUrl(node),

                            Studio = node.SelectSingleNode(".//ul[@class='anime-studios']/li/a")?
                                .InnerText.Trim(),

                            ReleaseDate = node.SelectSingleNode(".//div[@class='anime-date']")?
                                .InnerText.Trim(),

                            Source = node.SelectSingleNode(".//div[@class='anime-source']")?
                                .InnerText.Trim(),
                            Episodes = node.SelectSingleNode(".//div[@class='anime-episodes']")?
                                .InnerText.Trim(),

                            Synopsis = node.SelectSingleNode(".//div[@class='anime-synopsis']/p")?
                                .InnerText.Trim(),

                            Links = node.SelectNodes(".//div[@class='icon-buttons-set']/a")?
                                .ToDictionary(
                                    a => a.GetAttributeValue("class", "").Replace("-icon", ""),
                                    a => a.GetAttributeValue("href", "")
                                )
                        };

                        var episodeNode = node.SelectSingleNode(".//a[contains(@class, 'episode-countdown')]");
                        if (episodeNode != null)
                        {
                            anime.EpisodeInfo = new EpisodeInfo
                            {
                                EpisodeNumber = episodeNode.SelectSingleNode(".//div[@class='release-schedule-info']")?
                                    .InnerText.Trim(),
                                Countdown = episodeNode.SelectSingleNode(".//time")?
                                    .InnerText.Trim()
                            };
                        }

                        animeList.Add(anime);
                    }
                    catch 
                    {
                    }
                }

                return animeList;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Помилка при отриманні даних: {e.Message}");
                return new List<AnimeInfo>();
            }


        }

        private string GetPosterUrl(HtmlNode node)
        {
            var posterContainer = node.SelectSingleNode(".//div[contains(@class, 'poster-container')]");
            if (posterContainer == null) return null;

            var imgNode = posterContainer.SelectSingleNode(".//img");
            if (imgNode == null) return null;

            var srcset = imgNode.GetAttributeValue("srcset", "");
            if (!string.IsNullOrEmpty(srcset))
            {
                var urls = srcset.Split(',')
                                .Select(s => s.Trim().Split(' ')[0])
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList();

                if (urls.Any())
                {
                    return urls.Last();
                }
            }

            var dataSrc = imgNode.GetAttributeValue("data-src", "");
            if (!string.IsNullOrEmpty(dataSrc))
            {
                return dataSrc;
            }

            var src = imgNode.GetAttributeValue("src", "");
            if (!string.IsNullOrEmpty(src))
            {
                return src;
            }

            return null;
        }
    }

}
