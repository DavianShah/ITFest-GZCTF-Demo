using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GZCTF.Models.Request.Edit;

/// <summary>
/// Discord blood notification settings for a game
/// </summary>
public class BloodNotificationModel
{
    /// <summary>
    /// Enable Discord blood notifications
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Discord webhook URL
    /// </summary>
    [MaxLength(512)]
    public string? DiscordWebhookUrl { get; set; }

    /// <summary>
    /// Maximum rank to notify, or 0 for all first-solves
    /// </summary>
    public int MaxRank { get; set; } = 1;

    /// <summary>
    /// Notification embed description template
    /// </summary>
    [MaxLength(2000)]
    public string Template { get; set; } = Data.Game.DefaultBloodNotificationTemplate;

    [MaxLength(512)]
    public string EmbedTitleTemplate { get; set; } = Data.Game.DefaultBloodNotificationEmbedTitleTemplate;

    [MaxLength(2000)]
    public string EmbedDescriptionTemplate { get; set; } = Data.Game.DefaultBloodNotificationEmbedDescriptionTemplate;

    [MaxLength(16)]
    public string? EmbedColor { get; set; }

    [MaxLength(4000)]
    public string EmbedFieldsTemplate { get; set; } = Data.Game.DefaultBloodNotificationEmbedFieldsTemplate;

    [MaxLength(512)]
    public string EmbedFooterTemplate { get; set; } = Data.Game.DefaultBloodNotificationEmbedFooterTemplate;

    [MaxLength(64)]
    public string TimeZone { get; set; } = Data.Game.DefaultBloodNotificationTimeZone;

    internal static BloodNotificationModel FromGame(Data.Game game) =>
        new()
        {
            Enabled = game.BloodNotificationEnabled,
            DiscordWebhookUrl = game.BloodDiscordWebhookUrl,
            MaxRank = game.BloodNotificationMaxRank,
            Template = game.BloodNotificationTemplate,
            EmbedTitleTemplate = game.BloodNotificationEmbedTitleTemplate,
            EmbedDescriptionTemplate = game.BloodNotificationEmbedDescriptionTemplate,
            EmbedColor = game.BloodNotificationEmbedColor,
            EmbedFieldsTemplate = game.BloodNotificationEmbedFieldsTemplate,
            EmbedFooterTemplate = game.BloodNotificationEmbedFooterTemplate,
            TimeZone = game.BloodNotificationTimeZone
        };

    internal string? Validate(bool requireWebhook = false)
    {
        if (MaxRank is not (0 or 1 or 3 or 5))
            return "Max rank must be one of 0, 1, 3, or 5.";

        if ((Enabled || requireWebhook) && string.IsNullOrWhiteSpace(DiscordWebhookUrl))
            return "Discord webhook URL is required.";

        if (!string.IsNullOrWhiteSpace(DiscordWebhookUrl) &&
            (!Uri.TryCreate(DiscordWebhookUrl, UriKind.Absolute, out var uri) ||
             uri.Scheme is not ("http" or "https")))
            return "Discord webhook URL must be a valid HTTP or HTTPS URL.";

        if (Enabled && string.IsNullOrWhiteSpace(Template))
            return "Notification template is required when notifications are enabled.";

        if ((Enabled || requireWebhook) && string.IsNullOrWhiteSpace(EmbedTitleTemplate))
            return "Embed title template is required when notifications are enabled.";

        if ((Enabled || requireWebhook) && string.IsNullOrWhiteSpace(EmbedFooterTemplate))
            return "Embed footer template is required when notifications are enabled.";

        if (EmbedFieldsTemplate is null)
            return "Embed fields template cannot be null.";

        if (!string.IsNullOrWhiteSpace(EmbedColor) && !Regex.IsMatch(EmbedColor, "^#[0-9a-fA-F]{6}$"))
            return "Embed color must be empty or a six-digit hex color such as #ff0044.";

        if (string.IsNullOrWhiteSpace(TimeZone))
            return "Time zone is required.";

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return "Time zone was not found.";
        }
        catch (InvalidTimeZoneException)
        {
            return "Time zone is invalid.";
        }

        var lines = EmbedFieldsTemplate.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (lines.Length > 25)
            return "Embed fields template cannot contain more than 25 fields.";

        foreach (var line in lines)
        {
            var parts = line.Split('|');
            if (parts.Length is < 2 or > 3 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                return "Each embed field must use name|value|inline syntax.";

            if (parts.Length == 3 && !bool.TryParse(parts[2].Trim(), out _))
                return "Embed field inline values must be true or false.";
        }

        return null;
    }
}
