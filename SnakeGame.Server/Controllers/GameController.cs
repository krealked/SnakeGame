using Microsoft.AspNetCore.Mvc;
using SnakeGame.Shared.DTOs;
using SnakeGame.Server.Services;
namespace SnakeGame.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;
    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }
    [HttpPost("savescore")]
    public async Task<IActionResult> SaveScore([FromBody] SaveScoreRequest
    request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        await _gameService.SaveScoreAsync(request);
        return Ok(new { message = "Saved" });
    }
    [HttpGet("leaderboard")]
    public async Task<IActionResult> Leaderboard([FromQuery] int top = 10)
    {
        var res = await _gameService.GetLeaderboardAsync(top);
        return Ok(res);
    }
}