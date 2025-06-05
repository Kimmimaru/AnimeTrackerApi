using AnimeTrackerApi.Data.Repositories;
using AnimeTrackerApi.Models;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types.Enums;
using AnimeTrackerApi.Services;
using System.Text.RegularExpressions;

namespace AnimeTrackerApi.Bot.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly ILogger<NotificationService> _logger; // Правильне оголошення

        // У конструкторі:
        public NotificationService(IServiceProvider serviceProvider, ILogger<NotificationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger; // Ініціалізація
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);


        public NotificationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWorkAsync(stoppingToken);
        }

        public async Task ManualCheckAsync()
        {
            await DoWorkAsync(new CancellationToken());
        }

        private async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            // Перенесіть сюди всю логіку з ExecuteAsync
            _logger.LogInformation("🔔 Notification Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("🔍 Checking for releases...");

                    using var scope = _serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IExpectedAnimeRepository>();
                    var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

                    var now = DateTime.UtcNow;
                    var animeList = await repo.GetAllExpectedAnimeAsync();

                    _logger.LogInformation($"📊 Found {animeList.Count} anime in tracking list");

                    foreach (var anime in animeList)
                    {
                        _logger.LogDebug($"Checking: {anime.Title} (Release: {anime.ReleaseDate})");

                        if (anime.ReleaseDate.Date <= now.Date)
                        {
                            _logger.LogInformation($"🎬 Release today: {anime.Title} (User: {anime.UserId})");

                            try
                            {
                                await SendNotification(botClient, anime);
                                await repo.RemoveFromExpectedAsync(anime.Id, anime.UserId);
                                _logger.LogInformation($"✅ Notification sent for {anime.Title}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"❌ Failed to send notification for {anime.Title}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "⚠️ Error in release checker");
                }

                await Task.Delay(CheckInterval, stoppingToken);
            }
            
        }



        private async Task SendNotification(ITelegramBotClient botClient, ExpectedAnime anime)
        {
            try
            {
                var kyivTime = TimeZoneInfo.ConvertTimeFromUtc(
                    anime.ReleaseDate,
                    TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev"));

                // 1. Відправляємо сповіщення
                var message = $"🎉 <b>{anime.Title}</b> вийшло!\n" +
                             $"📅 Дата релізу: {kyivTime:dd.MM.yyyy HH:mm}\n" +
                             $"🔗 <a href=\"{anime.MalUrl}\">MyAnimeList</a>";

                await botClient.SendTextMessageAsync(
                    chatId: anime.UserId,
                    text: message,
                    parseMode: ParseMode.Html);

                // 2. Автоматично додаємо до watchlist
                using var scope = _serviceProvider.CreateScope();
                var watchlistRepo = scope.ServiceProvider.GetRequiredService<IWatchlistRepository>();
                var jikanService = scope.ServiceProvider.GetRequiredService<JikanService>();

                // Витягуємо MAL ID з посилання
                var malId = ExtractMalId(anime.MalUrl);

                if (malId > 0)
                {
                    var animeDetails = await jikanService.GetAnimeById(malId);

                    if (animeDetails != null)
                    {
                        var watchlistItem = new WatchlistItem
                        {
                            UserId = anime.UserId,
                            AnimeId = malId,
                            Title = animeDetails.GetBestAvailableTitle(),
                            Description = animeDetails.Synopsis,
                            PictureUrl = animeDetails.PictureUrl,
                            MyAnimeListUrl = animeDetails.MyAnimeListUrl ?? anime.MalUrl,
                            Status = "Watching", // Автоматично ставимо статус "Дивлюся"
                            AddedDate = DateTime.UtcNow
                        };

                        await watchlistRepo.AddToWatchlistAsync(watchlistItem);
                        _logger.LogInformation($"✅ Аніме {anime.Title} додано до watchlist");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Помилка при обробці аніме {anime.Title}");
            }
        }

        // Метод для витягування MAL ID з URL
        private int ExtractMalId(string malUrl)
        {
            if (string.IsNullOrEmpty(malUrl)) return 0;

            // Приклад URL: https://myanimelist.net/anime/5114/Fullmetal_Alchemist__Brotherhood
            var match = Regex.Match(malUrl, @"anime/(\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }
    }
}