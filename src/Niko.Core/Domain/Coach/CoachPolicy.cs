// ============================================================================
// Niko.Core — CoachPolicy.cs
// ----------------------------------------------------------------------------
// مسئولیت: اعمال محدودیت‌های ایمنی و حریم‌خصوصی بر زمینه و متن provider.
// وابستگی‌ها و لایه: Domain/Coach در Core؛ بدون شبکه، UI یا ذخیره‌سازی.
// نکات تغییر و قیود: raw notes/history، تشخیص، تجویز، تضمین نتیجه و لحن شرم‌آور
//           رد می‌شوند؛ این policy جایگزین ارزیابی پزشکی نیست.
// ============================================================================

namespace Niko.Core.Domain.Coach;

public static class CoachPolicy
{
    private static readonly string[] ProhibitedTerms =
    {
        "diagnos", "prescrib", "guarantee", "certain", "you will relapse",
        "shame", "weak", "must quit", "medical advice",
    };

    public static bool IsContextAllowed(CoachContext? context)
        => context is not null &&
           (context.CravingIntensity is null || context.CravingIntensity is >= 0 and <= 10) &&
           (context.ProgressPercent is null || context.ProgressPercent is >= 0 and <= 100) &&
           context.UserPreferences is not null &&
           context.UserPreferences.Count <= 8 &&
           context.UserPreferences.All(IsShortPreference) &&
           IsShortPreference(context.SelectedIntervention) &&
           IsShortPreference(context.MilestoneStatus);

    public static bool IsProviderTextAllowed(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length > 500)
        {
            return false;
        }

        var normalized = text.Trim().ToLowerInvariant();
        return !ProhibitedTerms.Any(normalized.Contains);
    }

    private static bool IsShortPreference(string? value)
        => value is null || (!string.IsNullOrWhiteSpace(value) && value.Length <= 40);
}
