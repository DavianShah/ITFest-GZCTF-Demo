namespace GZCTF.Models.Request.Game;

public class SpeedrunStateModel
{
    public bool IsSpeedrun { get; set; }
    public SpeedrunRoundModel? CurrentRound { get; set; }
    public ChallengeCategory[] UsedCategories { get; set; } = [];
    public ChallengeCategory[] RemainingCategories { get; set; } = [];
    public string? Message { get; set; }
}

public class SpeedrunRoundModel
{
    public int Id { get; set; }
    public ChallengeCategory Category { get; set; }
    public SpeedrunRoundStatus Status { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? EndsAtUtc { get; set; }
    public DateTimeOffset? OvertimeEndsAtUtc { get; set; }
    public int TimeLeftSeconds { get; set; }
    public bool IsOvertime => Status == SpeedrunRoundStatus.Overtime;
}
