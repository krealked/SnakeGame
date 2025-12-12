using SnakeGame.Client.Services;


namespace SnakeGame.Client.ViewModels;

public class MainViewModel
{
    private readonly IApiService _api;

    public string PlayerName { get; }
    public int Score { get; set; }

    public MainViewModel(string name, IApiService api)
    {
        PlayerName = name;
        _api = api;
    }

    public async Task SaveScoreAsync()
    {
        await _api.SaveScoreAsync(PlayerName, Score);
    }
}