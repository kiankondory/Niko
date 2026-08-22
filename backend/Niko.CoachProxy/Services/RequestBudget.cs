// ============================================================================
// نام فایل: RequestBudget.cs
// مسئولیت: محدودسازی سادهٔ درخواست‌های proxy در هر کلید client و روز UTC.
// وابستگی‌ها و لایه: Service در Backend؛ بدون ذخیره‌سازی یا دادهٔ کاربر.
// نکات تغییر و قیود: محدودیت محافظ abuse است و جایگزین rate limit زیرساخت production نیست.
// ============================================================================

using System.Collections.Concurrent;

namespace Niko.CoachProxy.Services;

public sealed class RequestBudget
{
    private readonly ConcurrentDictionary<string, Window> _windows = new(StringComparer.Ordinal);

    public bool TryAcquire(string clientKey, int perMinute, int perDay, DateTimeOffset now)
    {
        var window = _windows.GetOrAdd(clientKey, _ => new Window());
        lock (window)
        {
            var minute = now.ToUnixTimeSeconds() / 60;
            var day = now.UtcDateTime.Date;
            if (window.Minute != minute)
            {
                window.Minute = minute;
                window.MinuteCount = 0;
            }

            if (window.Day != day)
            {
                window.Day = day;
                window.DayCount = 0;
            }

            if (window.MinuteCount >= Math.Max(1, perMinute) || window.DayCount >= Math.Max(1, perDay))
            {
                return false;
            }

            window.MinuteCount++;
            window.DayCount++;
            return true;
        }
    }

    private sealed class Window
    {
        public long Minute { get; set; } = -1;
        public DateTime Day { get; set; } = DateTime.MinValue;
        public int MinuteCount { get; set; }
        public int DayCount { get; set; }
    }
}
