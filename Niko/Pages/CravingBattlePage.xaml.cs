// ============================================================================
// Niko.App — CravingBattlePage.xaml.cs
// ----------------------------------------------------------------------------
// مسئولیت: اتصال امن صفحهٔ Battle به ViewModel.
// وابستگی‌ها و لایه: MAUI presentation → CravingBattleViewModel؛ بدون منطق دامنه.
// نکات تغییر و قیود: lifecycle یا دادهٔ کاربر را تغییر نمی‌دهد.
// ============================================================================

using Niko.ViewModels;

namespace Niko.Pages
{
    public partial class CravingBattlePage : ContentPage
    {
        public CravingBattlePage(CravingBattleViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
