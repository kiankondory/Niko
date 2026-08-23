// ============================================================================
// Niko.App — AppMotionService.cs
// ----------------------------------------------------------------------------
// مسئولیت: نگهداری ترجیح محلی کاهش حرکت برای animationهای لایهٔ ارائه.
// وابستگی‌ها و لایه: MAUI presentation → Preferences؛ مستقل از Core، SQLite و رویدادها.
// نکات تغییر و قیود: این ترجیح فقط ظاهر را تغییر می‌دهد و هیچ دادهٔ سلامت یا
//           رفتار دامنه‌ای را ذخیره یا ارسال نمی‌کند.
// ============================================================================

using Microsoft.Maui.Storage;

namespace Niko.Services;

public interface IAppMotionService
{
    bool ReduceMotion { get; }

    void ApplyStoredPreference();

    void SetReduceMotion(bool reduceMotion);
}

public sealed class AppMotionService : IAppMotionService
{
    private const string PreferenceKey = "niko.reduce-motion";

    public bool ReduceMotion { get; private set; }

    public void ApplyStoredPreference()
        => ReduceMotion = Preferences.Default.Get(PreferenceKey, false);

    public void SetReduceMotion(bool reduceMotion)
    {
        ReduceMotion = reduceMotion;
        Preferences.Default.Set(PreferenceKey, reduceMotion);
    }
}
