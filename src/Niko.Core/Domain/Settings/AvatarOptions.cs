// ============================================================================
// Niko.Core — AvatarOptions.cs
// ----------------------------------------------------------------------------
// مسئولیت: فهرست آواتارهای انتخابی Niko و اعتبارسنجی شناسهٔ آن‌ها.
// وابستگی‌ها و لایه: Domain/Settings در Core؛ بدون فایل تصویری یا وابستگی به MAUI.
// نکات تغییر و قیود: Symbol یک نشانهٔ ارائه‌ای کنترل‌شده است؛ مسیر فایل به UI نشت نمی‌کند.
// ============================================================================

namespace Niko.Core.Domain.Settings;

public static class AvatarOptions
{
    public static IReadOnlyList<AvatarOption> All { get; } =
        new[]
        {
            new AvatarOption("niko-default", "Avatar.Default", "●"),
            new AvatarOption("niko-leaf", "Avatar.Leaf", "◆"),
            new AvatarOption("niko-sun", "Avatar.Sun", "✦"),
            new AvatarOption("niko-wave", "Avatar.Wave", "≈"),
        };

    public static bool IsSupported(string? avatarId)
        => avatarId is not null && All.Any(option =>
            string.Equals(option.Id, avatarId, StringComparison.Ordinal));
}
