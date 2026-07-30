using System.ComponentModel.DataAnnotations;
using GZCTF.Models.Request.Game;

namespace GZCTF.Models.Request.Edit;

public class SpeedrunSettingsModel
{
    [Range(1, 1440)] public int DefaultRoundDurationMinutes { get; set; } = 30;
    [Range(0, 120)] public int OvertimeMinutes { get; set; } = 5;
    [Range(1, 86400)] public int? DefaultRoundDurationSeconds { get; set; }
    [Range(0, 86400)] public int? OvertimeSeconds { get; set; }
    public bool AllowManualExtend { get; set; } = true;
    public bool HideInactiveChallenges { get; set; } = true;
    public bool EmergencyHintEnabled { get; set; } = true;
    [MaxLength(1000)] public string EmergencyHintText { get; set; } = string.Empty;
    public SpeedrunStateModel State { get; set; } = new();
    public SpeedrunCategoryModel[] Categories { get; set; } = [];
}

public class SpeedrunCategoryModel
{
    public int Id { get; set; }
    public ChallengeCategory Category { get; set; }
    public bool Used { get; set; }
    public bool Included { get; set; }
}

public class SpeedrunExtendModel
{
    [Range(1, 1440)] public int Minutes { get; set; } = 5;
    [Range(1, 86400)] public int? Seconds { get; set; }

    public int GetSeconds() => Seconds ?? Minutes * 60;
}

public class SpeedrunSetTimerModel
{
    [Range(1, 1440)] public int RemainingMinutes { get; set; } = 10;
    [Range(1, 86400)] public int? Seconds { get; set; }

    public int GetSeconds() => Seconds ?? RemainingMinutes * 60;
}
