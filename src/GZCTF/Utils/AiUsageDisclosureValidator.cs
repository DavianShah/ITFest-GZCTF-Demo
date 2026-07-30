namespace GZCTF.Utils;

public static class AiUsageDisclosureValidator
{
    public const string NoAiDeclaration = "Saya tidak memakai AI";

    public const string RequiredMessage =
        "Isi link AI atau pernyataan bahwa Anda tidak memakai AI sebelum mengirim flag.";

    public const string InvalidFormatMessage =
        "Isi harus berupa link percakapan AI dengan protokol http/https atau tepat \"Saya tidak memakai AI\".";

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim();
        if (normalized == NoAiDeclaration)
            return true;

        return Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
               && uri is { IsAbsoluteUri: true, Host.Length: > 0 }
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
