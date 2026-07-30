using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GZCTF.Models.Data;

[Index(nameof(TokenHash), IsUnique = true)]
[Index(nameof(GameId), nameof(CaptainEmail))]
public class CaptainOnboardingInvite
{
    [Key]
    public int Id { get; set; }

    public int GameId { get; set; }

    /// <summary>
    /// Other games selected in the same onboarding batch.
    /// GameId remains the primary game so the existing foreign key is preserved.
    /// </summary>
    public int[] AdditionalGameIds { get; set; } = [];

    [Required]
    [MaxLength(Limits.MaxTeamNameLength)]
    public string TeamName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string CaptainEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? ConsumedAtUtc { get; set; }

    /// <summary>
    /// First time the currently active link was opened.
    /// This may also be triggered by an email security link scanner.
    /// </summary>
    public DateTimeOffset? OpenedAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    public DateTimeOffset? LastSentAtUtc { get; set; }

    public int SendCount { get; set; }

    public bool LastEmailQueued { get; set; }

    public int? TeamId { get; set; }

    public Game Game { get; set; } = null!;

    public Team? Team { get; set; }

    public int[] GetGameIds() => [GameId, .. AdditionalGameIds.Where(id => id != GameId)];
}
