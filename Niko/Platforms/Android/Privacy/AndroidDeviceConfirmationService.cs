// ============================================================================
// Niko.App — AndroidDeviceConfirmationService.cs
// ----------------------------------------------------------------------------
// مسئولیت: درخواست PIN/Pattern/Password واقعی دستگاه Android پیش از حذف داده.
// وابستگی‌ها و لایه: Android adapter → IDeviceConfirmationService و MainActivity؛ بدون Core/SQLite.
// نکات تغییر و قیود: اگر قفل امن یا Activity در دسترس نباشد fail-closed است؛ هیچ راز یا PIN ثبت نمی‌شود.
// ============================================================================

using Android.App;
using Android.Content;
using Microsoft.Maui.ApplicationModel;
using Niko.Services;

namespace Niko.Platforms.Android.Privacy;

public sealed class AndroidDeviceConfirmationService : IDeviceConfirmationService
{
    public async Task<bool> ConfirmSensitiveActionAsync(
        string title,
        string description,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        return await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                return Task.FromResult(false);
            }

            var activity = Platform.CurrentActivity;
            var keyguard = activity?.GetSystemService(Context.KeyguardService) as KeyguardManager;
            if (activity is null || keyguard?.IsDeviceSecure != true)
            {
                return Task.FromResult(false);
            }

#pragma warning disable CA1422 // Android's device-credential confirmation remains the only PIN/pattern/password path without collecting a secret.
            var intent = keyguard.CreateConfirmDeviceCredentialIntent(title, description);
#pragma warning restore CA1422
            return intent is null
                ? Task.FromResult(false)
                : AndroidDeviceCredentialConfirmation.Begin(activity, intent, ct);
        }).ConfigureAwait(false);
    }
}

internal static class AndroidDeviceCredentialConfirmation
{
    internal const int RequestCode = 5401;
    private static readonly object Gate = new();
    private static PendingRequest? _pending;

    internal static Task<bool> Begin(Activity activity, Intent intent, CancellationToken ct)
    {
        PendingRequest request;
        lock (Gate)
        {
            _pending?.Complete(false);
            request = new PendingRequest(ct);
            _pending = request;
        }
        activity.StartActivityForResult(intent, RequestCode);
        return request.Task;
    }

    internal static void Complete(int requestCode, Result resultCode)
    {
        if (requestCode != RequestCode)
        {
            return;
        }

        lock (Gate)
        {
            _pending?.Complete(resultCode == Result.Ok);
            _pending = null;
        }
    }

    private sealed class PendingRequest
    {
        private readonly TaskCompletionSource<bool> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationTokenRegistration _registration;

        public PendingRequest(CancellationToken token)
        {
            _registration = token.Register(() => _completion.TrySetCanceled(token));
        }

        public Task<bool> Task => _completion.Task;

        public void Complete(bool value)
        {
            _registration.Dispose();
            _completion.TrySetResult(value);
        }
    }
}
