using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using SnakeGame.Client.Game;
using SnakeGame.Client.Services;

namespace SnakeGame.Client.Views;

public partial class MainWindow : Window
{
    private readonly string _playerName;
    private readonly GameEngine _engine;
    private readonly IApiService _api;

    public MainWindow(string playerName)
    {
        InitializeComponent();
        _playerName = playerName;
        PlayerNameText.Text = playerName;
        _api = App.AppHost!.Services.GetRequiredService<IApiService>();
        _engine = new GameEngine(GameCanvas, OnGameOver, UpdateScoreDisplay);
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
        KeyDown += MainWindow_KeyDown;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        GameCanvas.Focus();
        _engine.Start();
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        _engine.HandleKey(e.Key);
    }

    private void UpdateScoreDisplay(int score)
    {
        Dispatcher.Invoke(() => ScoreText.Text = $"Score: {score}");
    }

    private async void OnGameOver(int finalScore)
    {
        try
        {
            await _api.SaveScoreAsync(_playerName, finalScore);
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => MessageBox.Show($"Не удалось сохранить результат: {ex.Message}"));
        }
        Dispatcher.Invoke(() =>
        {
            MessageBox.Show($"Game over. Score: {finalScore}");
        });
    }

    private void OpenLeaderboard_Click(object sender, RoutedEventArgs e)
    {
        var win = new LeaderboardWindow();
        win.Owner = this;
        win.ShowDialog();
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _engine.Stop();
    }
}