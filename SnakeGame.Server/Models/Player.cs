using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace SnakeGame.Server.Models;
public class Player
{
    [Key]
    public int Id { get; set; }
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public ICollection<GameResult> Results { get; set; } = new
    List<GameResult>();
}