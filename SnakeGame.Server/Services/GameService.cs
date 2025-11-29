using Microsoft.EntityFrameworkCore;
using SnakeGame.Server.Data;
using SnakeGame.Server.Models;
using SnakeGame.Shared.DTOs;
namespace SnakeGame.Server.Services;
public class GameService : IGameService
{
    private readonly AppDbContext _db;
    private readonly ILogger<GameService> _logger;
    public GameService(AppDbContext db, ILogger<GameService> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task SaveScoreAsync(SaveScoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerName))
            throw new ArgumentException("Player name is required",
            nameof(request.PlayerName));

        //Normalize name

        var name = request.PlayerName.Trim();
        // Find or create player
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Name == name);
        if (player == null)
        {
            player = new Player { Name = name };
            _db.Players.Add(player);
            await _db.SaveChangesAsync(); // save to get Id
        }
        var result = new GameResult
        {
            PlayerId = player.Id,
            Score =
        request.Score,
            DateAchieved = DateTime.UtcNow
        };
        _db.GameResults.Add(result);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Saved score {Score} for player {Player}",
        request.Score, name);
    }
    public async Task<IEnumerable<LeaderboardItemDto>>
    GetLeaderboardAsync(int top = 10)
    {
        var query = _db.GameResults
        .Include(r => r.Player)
        .OrderByDescending(r => r.Score)
        .ThenBy(r => r.DateAchieved)
        .Take(top);
        return await query
        .Select(r => new LeaderboardItemDto(r.Player.Name, r.Score, r.DateAchieved))
        .ToListAsync();
    }
}