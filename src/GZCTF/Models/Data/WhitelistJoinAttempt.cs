using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GZCTF.Models.Data;

[Index(nameof(GameId), nameof(AttemptedAtUtc))]
[Index(nameof(TeamId), nameof(AttemptedAtUtc))]
public class WhitelistJoinAttempt
{
    [Key]
    public long Id { get; set; }

    public int GameId { get; set; }

    public int TeamId { get; set; }

    public Guid? UserId { get; set; }

    public DateTimeOffset AttemptedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Game Game { get; set; } = null!;

    public Team Team { get; set; } = null!;

    public UserInfo? User { get; set; }
}
