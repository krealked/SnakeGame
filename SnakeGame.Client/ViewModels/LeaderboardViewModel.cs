using System.Collections.ObjectModel;
using SnakeGame.Client.Services;
using SnakeGame.Shared.DTOs;

using System.Collections.ObjectModel;
using SnakeGame.Client.Services;

namespace SnakeGame.Client.ViewModels;

    public class LeaderboardViewModel
    {
        private readonly IApiService _api;

        public ObservableCollection<LeaderboardItemViewModel> Results { get; }
            = new();

        public LeaderboardViewModel(IApiService api)
        {
            _api = api;
            Load();
        }

         private async void Load()
         {
           var data = await _api.GetLeaderboardAsync(10); // получаем список DTO без Rank
           Results.Clear();

           int rank = 1;
           foreach (var r in data)
           {
               Results.Add(new LeaderboardItemViewModel
               {
                Rank = rank++,               // присваиваем ранк здесь
                PlayerName = r.PlayerName,
                Score = r.Score,
                DateAchieved = r.DateAchieved
               }   );
        }
          }

    }
