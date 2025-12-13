using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SnakeGame.Client.Services;
using SnakeGame.Client.ViewModels;

namespace SnakeGame.Client;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient<IApiService, ApiService>(c =>
                {
                    c.BaseAddress = new Uri(context.Configuration["Server:BaseUrl"] ?? "https://localhost:7266");
                });
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<LeaderboardViewModel>();
            })
            .Build();
        AppHost.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        AppHost?.Dispose();
        base.OnExit(e);
    }
}