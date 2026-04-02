using Microsoft.UI.Xaml.Controls;
using MovieShop.ViewModels;

namespace MovieShop.Views
{
    public sealed partial class WalletView : Page
    {
        public WalletViewModel ViewModel { get; }

        public WalletView(WalletViewModel viewModel)
        {
            ViewModel = viewModel;
            this.InitializeComponent();
            _ = ViewModel.LoadTransactionsAsync();
        }
    }
}
