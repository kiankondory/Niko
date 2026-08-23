// ============================================================================
// Niko.App — DashboardPage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: اتصال صفحهٔ داشبورد به ViewModel و اجرای animation کوتاهِ صرفاً
//           نمایشی برای نوار بهبود بدن پس از بارگذاری.
// وابستگی‌ها و لایه: MAUI Page → DashboardViewModel؛ داده و محاسبات فقط در Core/ViewModel هستند.
// نکات تغییر و قیود: animation مقدار دامنه را تغییر نمی‌دهد، مسیر offline را
//           دست‌کاری نمی‌کند و در صورت نبود دادهٔ recovery اجرا نمی‌شود.
// ============================================================================

using Niko.ViewModels;
using Niko.Services;

namespace Niko.Pages;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;
    private readonly IAppMotionService _motion;

    public DashboardPage(DashboardViewModel viewModel, IAppMotionService motion)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _motion = motion;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        await AnimateProgressAsync();
    }

    private async Task AnimateProgressAsync()
    {
        if (!_viewModel.IsRecoveryAvailable)
        {
            return;
        }

        if (_motion.ReduceMotion)
        {
            RecoveryProgressBar.Progress = _viewModel.RecoveryProgress;
            return;
        }

        RecoveryProgressBar.Progress = 0;
        await RecoveryProgressBar.ProgressTo(_viewModel.RecoveryProgress, 420, Easing.CubicOut);
    }
}
