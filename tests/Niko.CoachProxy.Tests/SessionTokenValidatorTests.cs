// ============================================================================
// نام فایل: SessionTokenValidatorTests.cs
// مسئولیت: آزمون fail-closed برای session token کوتاه‌عمر backend.
// وابستگی‌ها و لایه: Backend.Tests → SessionTokenValidator؛ بدون credential واقعی یا شبکه.
// نکات تغییر و قیود: token آزمایشی فقط در حافظه ساخته می‌شود و مقدار واقعی در خروجی ثبت نمی‌شود.
// ============================================================================

using System.Security.Cryptography;
using System.Text;
using Niko.CoachProxy.Services;

namespace Niko.CoachProxy.Tests;

public sealed class SessionTokenValidatorTests
{
    [Fact]
    public void ValidUnexpiredTokenIsAccepted()
    {
        const string secret = "test-session-secret";
        var expiry = DateTimeOffset.UtcNow.AddMinutes(2).ToUnixTimeSeconds().ToString();
        var token = $"{expiry}.{Sign(secret, expiry)}";

        Assert.True(new SessionTokenValidator(secret).IsValid(token));
    }

    [Fact]
    public void ExpiredOrWrongSecretIsRejected()
    {
        const string secret = "test-session-secret";
        var expiry = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds().ToString();
        var token = $"{expiry}.{Sign(secret, expiry)}";

        Assert.False(new SessionTokenValidator(secret).IsValid(token));
        Assert.False(new SessionTokenValidator("other-secret").IsValid(token));
        Assert.False(new SessionTokenValidator(string.Empty).IsValid(token));
    }

    private static string Sign(string secret, string value)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(value)))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }
}
