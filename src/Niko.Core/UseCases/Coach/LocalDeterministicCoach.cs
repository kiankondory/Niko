// ============================================================================
// Niko.Core — LocalDeterministicCoach.cs
// ----------------------------------------------------------------------------
// مسئولیت: تولید پیشنهادهای کوتاه و قطعی از زمینهٔ تجمیعی تأییدشده.
// وابستگی‌ها و لایه: UseCases/Coach در Core؛ فقط CoachContracts و CoachPolicy را مصرف می‌کند.
// نکات تغییر و قیود: بدون AI، شبکه، یادداشت خام یا مقدار حساس context؛ ورودی یکسان
//           خروجی یکسان دارد و متن خروجی فقط کلید محلی‌سازی است.
// ============================================================================

using Niko.Core.Domain.Coach;

namespace Niko.Core.UseCases.Coach;

public static class LocalDeterministicCoach
{
    public static CoachProviderResult Generate(CoachRequest request)
    {
        if (!CoachPolicy.IsContextAllowed(request.Context))
        {
            return CoachProviderResult.Failure(CoachProviderError.PolicyViolation);
        }

        if (request.Context.IsEmpty)
        {
            return CoachProviderResult.Failure(CoachProviderError.EmptyContext);
        }

        var suggestions = new List<CoachSuggestion>();
        if (request.Context.CravingIntensity is > 0)
        {
            suggestions.Add(new("local-craving-support", "Coach.Suggestion.CravingSupport", CoachSuggestionKind.CravingSupport));
        }

        if (request.Context.ProgressPercent is not null)
        {
            suggestions.Add(new("local-progress-encouragement", "Coach.Suggestion.Progress", CoachSuggestionKind.ProgressEncouragement));
        }

        if (!string.IsNullOrWhiteSpace(request.Context.MilestoneStatus))
        {
            suggestions.Add(new("local-milestone-encouragement", "Coach.Suggestion.Milestone", CoachSuggestionKind.MilestoneEncouragement));
        }

        return new CoachProviderResult(true, CoachProviderError.None, suggestions);
    }
}
