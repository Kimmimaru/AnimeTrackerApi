using AnimeTrackerApi.Data.Repositories;
using AnimeTrackerApi.Services;
using Microsoft.EntityFrameworkCore;
using AnimeTrackerApi.Data;
using AnimeTrackerApi.Bot.Services;
using Telegram.Bot;
using AnimeTrackerApi.Models;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<JikanService>();
builder.Services.AddScoped<JikanService>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<LiveChartService>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
});
builder.Services.AddScoped<LiveChartService>();


builder.Services.AddHostedService<NotificationService>();
builder.Services.AddScoped<IExpectedAnimeRepository, ExpectedAnimeRepository>();



builder.Logging.AddConsole();




builder.Services.AddHttpClient<JikanService>(client =>
{
    client.BaseAddress = new Uri("https://myanimelist.p.rapidapi.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); 
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
