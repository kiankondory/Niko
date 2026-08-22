// ============================================================================
// Niko.Core — CoachPreferences.cs
// ----------------------------------------------------------------------------
// مسئولیت: ترجیحات پایدار و حریم‌خصوصی مربی را تعریف می‌کند.
// وابستگی‌ها و لایه: Domain/Coach در Core؛ ذخیره‌سازی آن از طریق abstraction انجام می‌شود.
// نکات تغییر و قیود: همهٔ گزینه‌ها به‌صورت پیش‌فرض خاموش‌اند؛ حذف داده باید محلی و
//           قابل درخواست کاربر باشد و هیچ provider خارجی در این مرحله وجود ندارد.
// ============================================================================

namespace Niko.Core.Domain.Coach;

public sealed record CoachPreferences
{
    public bool Enabled { get; init; }
    public bool AllowExternalProvider { get; init; }
    public bool AllowAggregatedProgressContext { get; init; }
    public bool AllowCravingContext { get; init; }
}
