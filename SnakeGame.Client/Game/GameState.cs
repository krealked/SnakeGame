namespace SnakeGame.Client.Game;

public class GameState
{
    public int Width { get; set; }
    public int Height { get; set; }
    public Snake Snake { get; set; }
    public Point Food { get; set; }
    public bool IsGameOver { get; set; }
    public int Score { get; set; }
}