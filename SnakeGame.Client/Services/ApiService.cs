using System.Net.Http;
using System.Net.Http.Json;
using SnakeGame.Shared.DTOs;

namespace SnakeGame.Client.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _http;

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task SaveScoreAsync(string playerName, int score)
    {
        var req = new SaveScoreRequest(playerName, score);
        var res = await _http.PostAsJsonAsync("api/game/savescore", req);
        res.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<LeaderboardItemDto>> GetLeaderboardAsync(int top = 10)
    {
        var url = $"api/game/leaderboard?top={top}";
        var res = await _http.GetFromJsonAsync<IEnumerable<LeaderboardItemDto>>(url);
        return res ?? Enumerable.Empty<LeaderboardItemDto>();
    }
}