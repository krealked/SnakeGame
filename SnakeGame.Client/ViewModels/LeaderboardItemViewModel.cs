using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame.Client.ViewModels;

using System;
using System.ComponentModel;

public class LeaderboardItemViewModel : INotifyPropertyChanged
{
    private int _rank;
    public int Rank
    {
        get => _rank;
        set
        {
            if (_rank != value)
            {
                _rank = value;
                OnPropertyChanged(nameof(Rank));
            }
        }
    }

    public string PlayerName { get; set; }
    public int Score { get; set; }
    public DateTime DateAchieved { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
