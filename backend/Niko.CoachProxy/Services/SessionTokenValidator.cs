// ============================================================================
// نام فایل: SessionTokenValidator.cs
// مسئولیت: اعتبارسنجی session token کوتاه‌عمر با HMAC و secret backend.
// وابستگی‌ها و لایه: Service در Backend؛ فقط secret محیط اجرا و زمان UTC را مصرف می‌کند.
// نکات تغییر و قیود: token خام log نمی‌شود؛ token منقضی، malformed یا فاقد secret رد می‌شود.
// ============================================================================

using System.Security.Cryptography;
using System.Text;

namespace Niko.CoachProxy.Services;

public sealed class SessionTokenValidator
{
    private readonly string _secret;

    public SessionTokenValidator(string secret) => _secret = secret;

    public bool IsValid(string token)
    {
        if (string.IsNullOrWhiteSpace(_secret) || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.', 2);
        if (parts.Length != 2 || !long.TryParse(parts[0], out var expiry))
        {
            return false;
        }

        if (expiry <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            return false;
        }

        var expected = Sign(parts[0]);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(parts[1]));
    }

    private string Sign(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)))
            .Replace("+", "-", StringComparison.Ordinal)
            .Replace("/", "_", StringComparison.Ordinal)
            .TrimEnd('=');
    }
}
