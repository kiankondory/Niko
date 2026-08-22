// ============================================================================
// Niko.App — IslandPage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: اتصال صفحهٔ Island به ViewModel و بارگذاری امن snapshot هنگام نمایش.
// وابستگی‌ها و لایه: MAUI page → IslandViewModel؛ بدون منطق دامنه یا persistence.
// نکات تغییر و قیود: بارگذاری فقط خواندنی و آفلاین است و هیچ دادهٔ کاربر را تغییر نمی‌دهد.
// ============================================================================

using Niko.ViewModels;

namespace Niko.Pages;

public partial class IslandPage : ContentPage
{
    private readonly IslandViewModel _viewModel;

    public IslandPage(IslandViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
