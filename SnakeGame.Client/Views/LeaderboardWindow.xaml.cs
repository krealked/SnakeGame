using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SnakeGame.Client.Services;
using SnakeGame.Client.ViewModels;

namespace SnakeGame.Client.Views;

public partial class LeaderboardWindow : Window
{
    public LeaderboardWindow()
    {
        InitializeComponent();

        // Получаем сервис API
        var api = App.AppHost!.Services.GetRequiredService<IApiService>();

        // Создаём ViewModel, которая уже сама загрузит топ-10 и присвоит Rank
        var vm = new LeaderboardViewModel(api);

        // Назначаем DataContext для биндинга в XAML
        DataContext = vm;
    }
}
