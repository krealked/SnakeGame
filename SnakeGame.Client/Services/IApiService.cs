using SnakeGame.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnakeGame.Client.Services;

public interface IApiService
{
    Task SaveScoreAsync(string playerName, int score);
    Task<IEnumerable<LeaderboardItemDto>> GetLeaderboardAsync(int top = 10);
}