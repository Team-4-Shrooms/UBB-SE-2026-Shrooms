using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

namespace MovieShop.Views
{
    public sealed partial class BuyTicketPage : Page
    {
        private MovieEvent? @event;
        private readonly IEventRepository eventRepo = App.Services.GetRequiredService<IEventRepository>();
        private readonly IUserRepository userRepo = App.Services.GetRequiredService<IUserRepository>();
        private readonly IEventTicketService ticketService = App.Services.GetRequiredService<IEventTicketService>();

        public BuyTicketPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int id)
            {
                @event = eventRepo.GetEventById(id);
            }
            else if (e.Parameter is MovieEvent me)
            {
                @event = me;
            }

            if (@event == null)
            {
                return;
            }

            this.DataContext = @event;
            UpdateButtonState();
        }

        private void UpdateButtonState()
        {
            if (!SessionManager.IsLoggedIn)
            {
                ConfirmButton.IsEnabled = false;
                InsufficientText.Text = "You must be signed in to purchase.";
                InsufficientText.Visibility = Visibility.Visible;
                return;
            }

            if (@event == null)
            {
                ConfirmButton.IsEnabled = false;
                return;
            }

            var canBuy = ticketService.CanBuyTicket(SessionManager.CurrentUserID, @event);
            ConfirmButton.IsEnabled = canBuy;

            if (!canBuy)
            {
                InsufficientText.Text = $"Insufficient funds. Balance: {SessionManager.CurrentUserBalance:C} — Price: {@event.TicketPrice:C}";
                InsufficientText.Visibility = Visibility.Visible;
            }
            else
            {
                InsufficientText.Visibility = Visibility.Collapsed;
            }
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (@event == null)
            {
                return;
            }

            if (!SessionManager.IsLoggedIn)
            {
                var dlg = new ContentDialog
                {
                    Title = "Sign in",
                    Content = "Please sign in to continue.",
                    PrimaryButtonText = "Sign in",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };

                if (await dlg.ShowAsync() != ContentDialogResult.Primary)
                {
                    return;
                }

                SessionManager.CurrentUserID = 1;
                SessionManager.CurrentUserBalance = userRepo.GetBalance(1);
                UpdateButtonState();
                return;
            }

            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                    ticketService.PurchaseTicket(SessionManager.CurrentUserID, @event));

                UpdateButtonState();

                var dialog = new ContentDialog
                {
                    Title = "Purchase successful",
                    Content = $"Ticket for '{@event.Title}' purchased and added to your library.",
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();

                if (this.XamlRoot?.Content is NavigationPage navPage)
                {
                    navPage.ViewModel.RefreshWallet();
                    navPage.ViewModel.CurrentViewModel = "Inventory";
                }
            }
            catch (InvalidOperationException ex)
            {
                var err = new ContentDialog
                {
                    Title = "Cannot complete purchase",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = XamlRoot
                };
                await err.ShowAsync();
            }
        }
    }
}