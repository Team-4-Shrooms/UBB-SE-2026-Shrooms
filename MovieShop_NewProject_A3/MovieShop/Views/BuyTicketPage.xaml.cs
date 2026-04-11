using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.ViewModels;

namespace MovieShop.Views
{
    public sealed partial class BuyTicketPage : Page
    {
        public BuyTicketViewModel ViewModel { get; } = App.Services.GetRequiredService<BuyTicketViewModel>();

        public BuyTicketPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.Initialize(e.Parameter);
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedEvent = ViewModel.SelectedEvent;
            if (selectedEvent == null)
            {
                return;
            }

            if (!ViewModel.TryPurchase(out var error))
            {
                var errorDialog = new ContentDialog
                {
                    Title = "Cannot complete purchase",
                    Content = error,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot,
                };
                await errorDialog.ShowAsync();
                return;
            }

            var successDialog = new ContentDialog
            {
                Title = "Purchase successful",
                Content = $"Ticket for '{selectedEvent.Title}' purchased and added to your library.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            await successDialog.ShowAsync();

            if (XamlRoot?.Content is NavigationPage navPage)
            {
                navPage.ViewModel.RefreshWallet();
                navPage.ViewModel.CurrentViewModel = "Inventory";
            }
        }
    }
}
