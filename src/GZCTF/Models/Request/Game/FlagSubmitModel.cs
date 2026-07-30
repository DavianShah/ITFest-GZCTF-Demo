using System.ComponentModel.DataAnnotations;

namespace GZCTF.Models.Request.Game;

/// <summary>
/// Flag submission
/// </summary>
public class FlagSubmitModel
{
    /// <summary>
    /// Flag content
    /// </summary>
    [Required(ErrorMessageResourceName = nameof(Resources.Program.Model_FlagRequired),
        ErrorMessageResourceType = typeof(Resources.Program))]
    public string Flag { get; set; } = string.Empty;

    /// <summary>
    /// AI conversation link or declaration that AI was not used
    /// </summary>
    [MaxLength(Limits.MaxAiUsageDisclosureLength)]
    public string? AiUsageDisclosure { get; set; }
}
