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
