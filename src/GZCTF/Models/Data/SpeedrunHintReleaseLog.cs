using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GZCTF.Models.Data;

[Index(nameof(RoundId), nameof(ChallengeId), nameof(HintIndex), IsUnique = true)]
public class SpeedrunHintReleaseLog
{
    [Key]
    public int Id { get; set; }
    public int GameId { get; set; }
    public int RoundId { get; set; }
    public int ChallengeId { get; set; }
    public int HintIndex { get; set; }
    public DateTimeOffset ReleasedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public Game Game { get; set; } = null!;
    public SpeedrunRound Round { get; set; } = null!;
    public GameChallenge Challenge { get; set; } = null!;
}
