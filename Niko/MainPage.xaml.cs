using Niko.ViewModels;

// ============================================================================
// Niko.App — MainPage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: اتصال صفحهٔ ثبت سریع به MainViewModel.
// وابستگی‌ها و لایه: لایهٔ ارائهٔ MAUI → MainViewModel؛ بدون منطق دامنه، مسیر ناوبری یا ذخیره‌سازی.
// نکات تغییر و قیود: ناوبری مستقیم به Battle حذف شده تا مسیر معیوب از Quick Log اجرا نشود.
// ============================================================================

namespace Niko
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ((MainViewModel)BindingContext).LoadAsync();
        }

    }
}
