using SnakeGame.Server.Models;
using SnakeGame.Shared.DTOs;
namespace SnakeGame.Server.Services;
public interface IGameService
{
    Task SaveScoreAsync(SaveScoreRequest request);
    Task<IEnumerable<LeaderboardItemDto>> GetLeaderboardAsync(int top = 10);
}