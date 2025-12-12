using System.Windows;
using SnakeGame.Client.Views;

namespace SnakeGame.Client.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void Play_Click(object sender, RoutedEventArgs e)
    {
        var name = NameTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Введите имя");
            return;
        }

        var main = new MainWindow(name);
        main.Show();
        this.Close();
    }
}