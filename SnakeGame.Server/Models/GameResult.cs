using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SnakeGame.Server.Models;
public class GameResult
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(Player))]
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public int Score { get; set; }
    public DateTime DateAchieved { get; set; } = DateTime.UtcNow;
}