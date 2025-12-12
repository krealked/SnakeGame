using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SnakeGame.Client.Services;

namespace SnakeGame.Client.Views;

public partial class LeaderboardWindow : Window
{
    private readonly IApiService _api;

    public LeaderboardWindow()
    {
        InitializeComponent();
        _api = App.AppHost!.Services.GetRequiredService<IApiService>();
        Loaded += LeaderboardWindow_Loaded;
    }

    private async void LeaderboardWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var list = await _api.GetLeaderboardAsync(10);
            Grid.ItemsSource = list;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить таблицу лидеров: {ex.Message}");
        }
    }
}