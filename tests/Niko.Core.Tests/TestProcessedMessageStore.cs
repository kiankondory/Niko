// ============================================================================
// Niko.Core.Tests — TestProcessedMessageStore.cs
// ----------------------------------------------------------------------------
// مسئولیت: پیاده‌سازی درون‌حافظهٔ IProcessedMessageStore برای تست مورد کاربرد
//           پیام ابزارک/ساعت.
// وابستگی‌ها و لایه: لایهٔ تست؛ قرارداد Core را پیاده می‌کند.
// نکات تغییر و قیود: فقط برای تست.
// ============================================================================

using System.Collections.Concurrent;
using Niko.Core.Abstractions;

namespace Niko.Core.Tests;

/// <summary>
/// ذخیره‌ساز درون‌حافظهٔ شناسهٔ پیام‌های پردازش‌شده.
/// </summary>
public sealed class TestProcessedMessageStore : IProcessedMessageStore
{
    private readonly ConcurrentDictionary<string, byte> _processed = new(StringComparer.Ordinal);

    public Task<bool> TryMarkProcessedAsync(string messageId, CancellationToken ct = default)
        => Task.FromResult(_processed.TryAdd(messageId, 0));
}
