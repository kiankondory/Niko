// ============================================================================
// Niko.Core — UserProfile.cs
// ----------------------------------------------------------------------------
// مسئولیت: مدل تنظیمات/پروفایل کاربر برای محاسبهٔ پیشرفت و صرفه‌جویی. از داده‌های
//           مصرف روزانه، قیمت (هر نخ یا هر بسته با اندازهٔ بسته)، ارز و تاریخ ترک
//           برای محاسبهٔ صرفه‌جویی تقریبی استفاده می‌شود.
// وابستگی‌ها و لایه: بخش Domain در Core؛ بدون وابستگی به پلتفرم.
// نکات تغییر و قیود: فیلدهای مالی اختیاری‌اند. قیمت مؤثر هر نخ از «قیمت هر نخ»
//           یا «قیمت هر بسته ÷ اندازهٔ بسته» به‌دست می‌آید. همهٔ مقادیر باید مثبت
//           باشند تا صرفه‌جویی محاسبه شود. جنبهٔ تشخیصی ندارد.
// ============================================================================

namespace Niko.Core.Domain;

/// <summary>
/// پروفایل و تنظیمات کاربر برای محاسبهٔ پیشرفت و صرفه‌جویی.
/// </summary>
public sealed record UserProfile
{
    /// <summary>نام نمایشی اختیاری کاربر.</summary>
    public string? DisplayName { get; init; }

    /// <summary>شناسهٔ آواتار انتخاب‌شده؛ مسیر تصویر نیست.</summary>
    public string AvatarId { get; init; } = "niko-default";

    /// <summary>تاریخ ترک (UTC).</summary>
    public DateTimeOffset? QuitDateUtc { get; init; }

    /// <summary>میانگین مصرف روزانه (برای صرفه‌جویی تقریبی).</summary>
    public int? CigarettesPerDay { get; init; }

    /// <summary>قیمت تقریبی هر نخ.</summary>
    public decimal? PricePerCigarette { get; init; }

    /// <summary>قیمت تقریبی هر بسته.</summary>
    public decimal? PricePerPack { get; init; }

    /// <summary>تعداد نخ داخل هر بسته.</summary>
    public int? PackSize { get; init; }

    /// <summary>کد ارز (مانند USD، EUR) برای قالب‌بندی محلی.</summary>
    public string CurrencyCode { get; init; } = "USD";

    /// <summary>locale ترجیحی کاربر.</summary>
    public string? PreferredLocale { get; init; }

    /// <summary>
    /// قیمت مؤثر هر نخ: از قیمت هر نخ، یا (قیمت هر بسته ÷ اندازهٔ بسته).
    /// اگر هیچ‌کدام معتبر نباشد null برمی‌گرداند.
    /// </summary>
    public decimal? EffectivePricePerCigarette
    {
        get
        {
            if (PricePerCigarette is > 0)
            {
                return PricePerCigarette;
            }

            if (PricePerPack is > 0 && PackSize is > 0)
            {
                return PricePerPack / PackSize.Value;
            }

            return null;
        }
    }

    /// <summary>
    /// آیا دادهٔ لازم برای محاسبهٔ صرفه‌جویی موجود است؟
    /// </summary>
    public bool HasSavingsInput =>
        QuitDateUtc is not null &&
        CigarettesPerDay is > 0 &&
        EffectivePricePerCigarette is > 0;
}
