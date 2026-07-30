using GZCTF.Utils;
using Xunit;

namespace GZCTF.Test.UnitTests.Utils;

public class AiUsageDisclosureValidatorTests
{
    [Theory]
    [InlineData("Saya tidak memakai AI")]
    [InlineData("https://chatgpt.com/share/example")]
    [InlineData("http://localhost/conversation")]
    [InlineData("  https://claude.ai/share/example  ")]
    public void ValidDisclosure_IsAccepted(string value)
    {
        Assert.True(AiUsageDisclosureValidator.IsValid(value));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("aa")]
    [InlineData("Saya tidak memakai AI dalam menyelesaikan challenge ini.")]
    [InlineData("chatgpt.com/share/example")]
    [InlineData("ftp://example.com/conversation")]
    [InlineData("javascript:alert(1)")]
    [InlineData("<img src=x onerror=alert(1)>")]
    public void InvalidDisclosure_IsRejected(string? value)
    {
        Assert.False(AiUsageDisclosureValidator.IsValid(value));
    }
}
