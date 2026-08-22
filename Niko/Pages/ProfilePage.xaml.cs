// ============================================================================
// Niko.App — ProfilePage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: آداپتر صفحهٔ پروفایل و هاب تنظیمات. فرم و navigation را به
//           SettingsViewModel می‌سپارد و هیچ قاعدهٔ دامنه‌ای ندارد.
// وابستگی‌ها و لایه: MAUI UI → SettingsViewModel → Core use cases/stores.
// نکات تغییر و قیود: همهٔ متن‌ها از bindingهای محلی‌سازی می‌آیند؛ ذخیره آفلاین است.
// ============================================================================

using Niko.ViewModels;

namespace Niko.Pages;

public partial class ProfilePage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public ProfilePage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
