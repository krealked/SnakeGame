using System.ComponentModel.DataAnnotations;
namespace SnakeGame.Server.DTOs;
public class SaveScoreRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string PlayerName { get; set; } = null!;
    [Range(0, int.MaxValue)]
    public int Score { get; set; }
}