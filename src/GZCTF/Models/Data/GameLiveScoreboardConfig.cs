using System.ComponentModel.DataAnnotations;

namespace GZCTF.Models.Data;

public class GameLiveScoreboardConfig
{
    [Key] public int GameId { get; set; }
    public bool Enabled { get; set; }
    [MaxLength(128)] public string Title { get; set; } = "ITFest Live Scoreboard";
    [MaxLength(256)] public string? Subtitle { get; set; }
    public bool SoundEnabled { get; set; } = true;
    [Range(0, 1)] public double Volume { get; set; } = 0.75;
    public LiveScoreboardVisualIntensity VisualIntensity { get; set; } = LiveScoreboardVisualIntensity.Normal;
    [MaxLength(512)] public string? SoundSpin { get; set; }
    [MaxLength(512)] public string? SoundCategorySelected { get; set; }
    [MaxLength(512)] public string? SoundGameStart { get; set; }
    [MaxLength(512)] public string? SoundHintDrop { get; set; }
    [MaxLength(512)] public string? SoundFirstBlood { get; set; }
    [MaxLength(512)] public string? SoundSecondBlood { get; set; }
    [MaxLength(512)] public string? SoundThirdBlood { get; set; }
    [MaxLength(512)] public string? SoundCorrectSubmit { get; set; }
    [MaxLength(512)] public string? SoundWrongSubmit { get; set; }
    [MaxLength(512)] public string? SoundReminder { get; set; }
    [MaxLength(512)] public string? SoundCountdownTick { get; set; }
    [MaxLength(512)] public string? SoundOvertime { get; set; }
    [MaxLength(512)] public string? SoundRoundFinished { get; set; }
    [MaxLength(512)] public string? SoundScoreUpdate { get; set; }
    public Game Game { get; set; } = null!;
}
