// ============================================================================
// Niko.Infrastructure — InMemoryProcessedMessageStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی درون‌حافظهٔ IProcessedMessageStore برای جلوگیری از پردازش
//           تکراری پیام‌های ابزارک/ساعت در یک نشست. در نسخهٔ بعدی می‌توان آن را
//           با SQLite جایگزین کرد تا بین راه‌اندازی‌ها پایدار بماند.
// وابستگی‌ها و لایه: Infrastructure/Persistence → Core.Abstractions.
// نکات تغییر و قیود: فقط درون‌حافظه و برای این مرحله کافی است؛ idempotency بین
//           راه‌اندازی‌ها نیازمند پیاده‌سازی پایدار است (در DECISIONS ثبت می‌شود).
// ============================================================================

using System.Collections.Concurrent;
using Niko.Core.Abstractions;

namespace Niko.Infrastructure.Persistence;

/// <summary>
/// ذخیره‌ساز درون‌حافظهٔ شناسهٔ پیام‌های پردازش‌شده.
/// </summary>
public sealed class InMemoryProcessedMessageStore : IProcessedMessageStore
{
    private readonly ConcurrentDictionary<string, byte> _processed = new(StringComparer.Ordinal);

    public Task<bool> TryMarkProcessedAsync(string messageId, CancellationToken ct = default)
        => Task.FromResult(_processed.TryAdd(messageId, 0));
}
