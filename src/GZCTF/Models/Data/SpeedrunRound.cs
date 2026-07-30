using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GZCTF.Models.Data;

[Index(nameof(GameId), nameof(Status))]
public class SpeedrunRound
{
    [Key]
    public int Id { get; set; }
    public int GameId { get; set; }
    public ChallengeCategory Category { get; set; }
    public SpeedrunRoundStatus Status { get; set; } = SpeedrunRoundStatus.Ready;
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? EndsAtUtc { get; set; }
    public DateTimeOffset? OvertimeEndsAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public int OvertimeMinutes { get; set; }
    public int DurationSeconds { get; set; }
    public int OvertimeSeconds { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAtUtc { get; set; }
    public Guid? SelectedByUserId { get; set; }
    public int ManuallyExtendedMinutes { get; set; }
    public int ManuallyExtendedSeconds { get; set; }
    public bool OvertimeNoticeSent { get; set; }
    public Game Game { get; set; } = null!;
}
