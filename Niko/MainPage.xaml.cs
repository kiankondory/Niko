using Niko.ViewModels;

namespace Niko
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnCravingBattleClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("CravingBattlePage");
        }

    }
}
