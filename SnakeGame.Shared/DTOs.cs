namespace SnakeGame.Shared.DTOs;
public record SaveScoreRequest(string PlayerName, int Score);
public record LeaderboardItemDto(string PlayerName, int Score, DateTime
DateAchieved);
// Optionally shared domain models
public record PlayerDto(int Id, string Name, DateTime CreatedDate);
public record GameResultDto(int Id, int PlayerId, int Score, DateTime
DateAchieved);