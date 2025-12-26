namespace SnakeGame.Client.Game;

public class GameState
{
    public int Width { get; set; }
    public int Height { get; set; }
    public required Snake Snake { get; set; }
    public required Point Food { get; set; }
    public bool IsGameOver { get; set; }
    public int Score { get; set; }
}