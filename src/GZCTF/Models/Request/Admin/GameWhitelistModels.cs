using System.ComponentModel.DataAnnotations;
namespace GZCTF.Models.Request.Admin;

public class WhitelistTeamsRequest
{
    [Required, MinLength(1)]
    public int[] TeamIds { get; set; } = [];
}

public class WhitelistTeamModel
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? CaptainEmail { get; set; }
    public WhitelistSource Source { get; set; }
    public ParticipationStatus Status { get; set; }
}

public class WhitelistJoinAttemptModel
{
    public long Id { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public DateTimeOffset AttemptedAtUtc { get; set; }
}

public class CaptainOnboardingEntryModel
{
    [Required, MaxLength(Limits.MaxTeamNameLength)]
    public string TeamName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string CaptainEmail { get; set; } = string.Empty;
}

public class CaptainOnboardingBatchModel
{
    [Required, MinLength(1)]
    public CaptainOnboardingEntryModel[] Entries { get; set; } = [];

    /// <summary>
    /// Games that the provisioned teams will be whitelisted for.
    /// Required by the global onboarding endpoint.
    /// </summary>
    public int[] GameIds { get; set; } = [];

    [Range(1, 168)]
    public int ExpiresInHours { get; set; } = 72;
}

public class CaptainOnboardingCreatedModel
{
    public int InviteId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string CaptainEmail { get; set; } = string.Empty;
    public string OnboardingUrl { get; set; } = string.Empty;
    public bool EmailQueued { get; set; }
    public string[] GameTitles { get; set; } = [];
}

public class CaptainOnboardingRecordModel
{
    public int InviteId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string CaptainEmail { get; set; } = string.Empty;
    public string[] GameTitles { get; set; } = [];
    public CaptainOnboardingStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? OpenedAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public DateTimeOffset? LastSentAtUtc { get; set; }
    public int SendCount { get; set; }
    public bool LastEmailQueued { get; set; }
    public int? TeamId { get; set; }
}

public class CaptainOnboardingResendModel
{
    [Range(1, 168)]
    public int ExpiresInHours { get; set; } = 72;
}

public class CaptainOnboardingInfoModel
{
    public string TeamName { get; set; } = string.Empty;
    public string CaptainEmail { get; set; } = string.Empty;
    public string GameTitle { get; set; } = string.Empty;
    public string[] GameTitles { get; set; } = [];
    public DateTimeOffset ExpiresAtUtc { get; set; }
}

public class CaptainOnboardingRedeemModel
{
    [Required]
    [MinLength(Limits.MinUserNameLength)]
    [MaxLength(Limits.MaxUserNameLength)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class CaptainOnboardingRedeemResultModel
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public string[] GameTitles { get; set; } = [];
}
