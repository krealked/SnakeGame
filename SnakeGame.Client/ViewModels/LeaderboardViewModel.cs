using System.Collections.ObjectModel;
using SnakeGame.Client.Services;
using SnakeGame.Shared.DTOs;

namespace SnakeGame.Client.ViewModels;

public class LeaderboardViewModel
{
    private readonly IApiService _api;
    public ObservableCollection<LeaderboardItemDto> Results { get; set; } = new();

    public LeaderboardViewModel(IApiService api)
    {
        _api = api;
        Load();
    }

    private async void Load()
    {
        var data = await _api.GetLeaderboardAsync(10);
        Results.Clear();
        foreach (var r in data)
            Results.Add(r);
    }
}