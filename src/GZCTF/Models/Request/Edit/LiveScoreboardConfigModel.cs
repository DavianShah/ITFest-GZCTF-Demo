using System.ComponentModel.DataAnnotations;

namespace GZCTF.Models.Request.Edit;

public class LiveScoreboardConfigModel
{
    public bool Enabled { get; set; }
    [Required, MaxLength(128)] public string Title { get; set; } = "ITFest Live Scoreboard";
    [MaxLength(256)] public string? Subtitle { get; set; }
    public bool SoundEnabled { get; set; } = true;
    [Range(0, 1)] public double Volume { get; set; } = 0.75;
    public LiveScoreboardVisualIntensity VisualIntensity { get; set; } = LiveScoreboardVisualIntensity.Normal;
    public LiveScoreboardSoundModel Sounds { get; set; } = new();

    public static LiveScoreboardConfigModel FromConfig(GameLiveScoreboardConfig? config) => config is null ? new() : new()
    {
        Enabled = config.Enabled, Title = config.Title, Subtitle = config.Subtitle, SoundEnabled = config.SoundEnabled,
        Volume = config.Volume, VisualIntensity = config.VisualIntensity, Sounds = LiveScoreboardSoundModel.FromConfig(config)
    };

    public void Apply(GameLiveScoreboardConfig config)
    {
        config.Enabled = Enabled; config.Title = Title; config.Subtitle = Subtitle; config.SoundEnabled = SoundEnabled;
        config.Volume = Volume; config.VisualIntensity = VisualIntensity; (Sounds ?? new()).Apply(config);
    }
}

public class LiveScoreboardSoundModel
{
    public string? Spin { get; set; }
    public string? CategorySelected { get; set; }
    public string? GameStart { get; set; }
    public string? HintDrop { get; set; }
    public string? FirstBlood { get; set; }
    public string? SecondBlood { get; set; }
    public string? ThirdBlood { get; set; }
    public string? CorrectSubmit { get; set; }
    public string? WrongSubmit { get; set; }
    public string? Reminder { get; set; }
    public string? CountdownTick { get; set; }
    public string? Overtime { get; set; }
    public string? RoundFinished { get; set; }
    public string? ScoreUpdate { get; set; }

    internal static LiveScoreboardSoundModel FromConfig(GameLiveScoreboardConfig c) => new()
    {
        Spin = c.SoundSpin, CategorySelected = c.SoundCategorySelected, GameStart = c.SoundGameStart,
        HintDrop = c.SoundHintDrop, FirstBlood = c.SoundFirstBlood, SecondBlood = c.SoundSecondBlood,
        ThirdBlood = c.SoundThirdBlood, CorrectSubmit = c.SoundCorrectSubmit, WrongSubmit = c.SoundWrongSubmit,
        Reminder = c.SoundReminder, CountdownTick = c.SoundCountdownTick,
        Overtime = c.SoundOvertime, RoundFinished = c.SoundRoundFinished, ScoreUpdate = c.SoundScoreUpdate
    };

    internal void Apply(GameLiveScoreboardConfig c)
    {
        c.SoundSpin = Spin; c.SoundCategorySelected = CategorySelected; c.SoundGameStart = GameStart;
        c.SoundHintDrop = HintDrop; c.SoundFirstBlood = FirstBlood; c.SoundSecondBlood = SecondBlood;
        c.SoundThirdBlood = ThirdBlood; c.SoundCorrectSubmit = CorrectSubmit; c.SoundWrongSubmit = WrongSubmit;
        c.SoundReminder = Reminder; c.SoundCountdownTick = CountdownTick;
        c.SoundOvertime = Overtime; c.SoundRoundFinished = RoundFinished; c.SoundScoreUpdate = ScoreUpdate;
    }
}
