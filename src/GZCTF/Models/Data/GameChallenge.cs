using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GZCTF.Models.Request.Edit;

namespace GZCTF.Models.Data;

public class GameChallenge : Challenge
{
    /// <summary>
    /// Whether to record traffic
    /// </summary>
    public bool EnableTrafficCapture { get; set; }

    /// <summary>
    /// Whether to disable blood bonus
    /// </summary>
    public bool DisableBloodBonus { get; set; }

    /// <summary>
    /// Whether Jeopardy submissions must include a solver file
    /// </summary>
    public bool RequireSolverUpload { get; set; }

    /// <summary>
    /// Speedrun hint release minute by hint index. Missing entries default to minute 0.
    /// </summary>
    public List<int>? SpeedrunHintReleaseMinutes { get; set; }

    /// <summary>
    /// Speedrun hint release second by hint index. Missing entries default to second 0.
    /// </summary>
    public List<int>? SpeedrunHintReleaseSeconds { get; set; }

    /// <summary>
    /// Initial score
    /// </summary>
    [Required]
    public int OriginalScore { get; set; } = 1000;

    /// <summary>
    /// Minimum score rate
    /// </summary>
    [Required]
    [Range(0, 1)]
    public double MinScoreRate { get; set; } = 0.25;

    /// <summary>
    /// Difficulty coefficient
    /// </summary>
    [Required]
    public double Difficulty { get; set; } = 5;

    /// <summary>
    /// Current score of the challenge
    /// </summary>
    [NotMapped]
    public int CurrentScore => CalculateChallengeScore(
        OriginalScore,
        MinScoreRate,
        Difficulty,
        FirstSolves?.Count ?? 0);


    internal static int CalculateChallengeScore(int originalScore, double minScoreRate, double difficulty,
        int acceptedCount)
    {
        if (acceptedCount <= 1)
            return originalScore;

        return (int)Math.Floor(
            originalScore *
            (minScoreRate + (1.0 - minScoreRate) * Math.Exp((1 - acceptedCount) / difficulty)));
    }

    internal void Update(ChallengeUpdateModel model)
    {
        Title = model.Title ?? Title;
        Content = model.Content ?? Content;
        Category = model.Category ?? Category;
        if (model.Hints is not null)
        {
            Hints = model.Hints;
            var secondSchedule = model.SpeedrunHintReleaseSeconds ?? SpeedrunHintReleaseSeconds ??
                                 (model.SpeedrunHintReleaseMinutes ?? SpeedrunHintReleaseMinutes ?? [])
                                 .Select(minute => minute * 60).ToList();
            SpeedrunHintReleaseSeconds = Enumerable.Range(0, Hints.Count)
                .Select(index => secondSchedule.ElementAtOrDefault(index))
                .ToList();
            SpeedrunHintReleaseMinutes = SpeedrunHintReleaseSeconds.Select(second => second / 60).ToList();
        }
        else if (model.SpeedrunHintReleaseSeconds is not null)
        {
            SpeedrunHintReleaseSeconds = model.SpeedrunHintReleaseSeconds;
            SpeedrunHintReleaseMinutes = model.SpeedrunHintReleaseSeconds.Select(second => second / 60).ToList();
        }
        else if (model.SpeedrunHintReleaseMinutes is not null)
        {
            SpeedrunHintReleaseMinutes = model.SpeedrunHintReleaseMinutes;
            SpeedrunHintReleaseSeconds = model.SpeedrunHintReleaseMinutes.Select(minute => minute * 60).ToList();
        }
        CPUCount = model.CPUCount ?? CPUCount;
        MemoryLimit = model.MemoryLimit ?? MemoryLimit;
        StorageLimit = model.StorageLimit ?? StorageLimit;
        ContainerImage = model.ContainerImage?.Trim() ?? ContainerImage;
        ExposePort = model.ExposePort ?? ExposePort;
        NetworkMode = model.NetworkMode ?? NetworkMode;
        OriginalScore = model.OriginalScore ?? OriginalScore;
        MinScoreRate = model.MinScoreRate ?? MinScoreRate;
        Difficulty = model.Difficulty ?? Difficulty;
        FileName = model.FileName ?? FileName;
        DisableBloodBonus = model.DisableBloodBonus ?? DisableBloodBonus;
        RequireSolverUpload = model.RequireSolverUpload ?? RequireSolverUpload;
        SubmissionLimit = model.SubmissionLimit ?? SubmissionLimit;

        // isEnabled should be updated alone
        IsEnabled = model.IsEnabled ?? IsEnabled;

        // only set DeadlineUtc to null when pass DateTimeOffset.MinValue (but not null)
        if (model.DeadlineUtc is { } time)
            DeadlineUtc = time.ToUnixTimeSeconds() == 0 ? null : time;

        // only set FlagTemplate to null when pass an empty string (but not null)
        if (model.FlagTemplate is { } template)
            FlagTemplate = string.IsNullOrWhiteSpace(template) ? null : template;

        // Container only
        EnableTrafficCapture = Type.IsContainer() && (model.EnableTrafficCapture ?? EnableTrafficCapture);
    }

    #region Db Relationship

    /// <summary>
    /// Submissions
    /// </summary>
    public List<Submission> Submissions { get; set; } = [];

    /// <summary>
    /// Challenge instances
    /// </summary>
    public List<GameInstance> Instances { get; set; } = [];

    /// <summary>
    /// Teams that activated the challenge
    /// </summary>
    public HashSet<Participation> Teams { get; set; } = [];

    /// <summary>
    /// Configurations for divisions
    /// </summary>
    public HashSet<DivisionChallengeConfig> DivisionConfigs { get; set; } = [];

    /// <summary>
    /// First solves recorded for this challenge.
    /// </summary>
    public List<FirstSolve>? FirstSolves { get; set; } = [];

    /// <summary>
    /// Game ID
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// Game object
    /// </summary>
    public Game Game { get; set; } = null!;

    #endregion Db Relationship
}
