using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

namespace MovieShop.Views
{
    public sealed partial class InventoryView : Page
    {
        private readonly IInventoryRepository repo = App.Services.GetRequiredService<IInventoryRepository>();
        private readonly IUserRepository userRepo = App.Services.GetRequiredService<IUserRepository>();
        private readonly IInventoryService inventoryService = App.Services.GetRequiredService<IInventoryService>();

        public InventoryView()
        {
            InitializeComponent();
            Loaded += InventoryView_Loaded;
            MoviesGrid.ItemClick += MoviesGrid_ItemClick;
        }

        private void MoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie movie)
            {
                Frame?.Navigate(typeof(MovieDetailPage), new MovieDetailNavArgs { Movie = movie, MainViewModel = null! });
            }
        }

        private void InventoryView_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var userId = SessionManager.CurrentUserID;
            if (userId <= 0)
            {
                return;
            }

            var movies = repo.GetOwnedMovies(userId);
            MoviesGrid.ItemsSource = movies;

            var tickets = repo.GetOwnedTickets(userId);
            TicketsGrid.ItemsSource = tickets;

            var equipment = repo.GetOwnedEquipment(userId);
            EquipmentGrid.ItemsSource = equipment;
        }

        private async void RemoveMovie_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
            => await RemoveMovieAsync(sender);

        private async Task RemoveMovieAsync(object sender)
        {
            if (sender is Microsoft.UI.Xaml.FrameworkElement element && element.DataContext is Movie movie)
            {
                var dlg = new ContentDialog
                {
                    Title = "Remove movie",
                    Content = $"Are you sure you want to remove '{movie.Title}' from your library? This will allow you to purchase it again.",
                    PrimaryButtonText = "Remove",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };

                if (await dlg.ShowAsync() != ContentDialogResult.Primary)
                {
                    return;
                }

                MoviesGrid.ItemsSource = inventoryService.RemoveMovie(SessionManager.CurrentUserID, movie.ID);

                if (this.XamlRoot?.Content is NavigationPage navPage)
                {
                    navPage.ViewModel.RefreshWallet();
                }
            }
        }

        private async void RemoveTicket_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
            => await RemoveTicketAsync(sender);

        private async Task RemoveTicketAsync(object sender)
        {
            if (sender is Microsoft.UI.Xaml.FrameworkElement element && element.DataContext is MovieEvent movieEvent)
            {
                var dlg = new ContentDialog
                {
                    Title = "Remove ticket",
                    Content = $"Remove your ticket for '{movieEvent.Title}'? This will allow you to buy it again.",
                    PrimaryButtonText = "Remove",
                    CloseButtonText = "Cancel",
                    XamlRoot = XamlRoot
                };

                if (await dlg.ShowAsync() != ContentDialogResult.Primary)
                {
                    return;
                }

                TicketsGrid.ItemsSource = inventoryService.RemoveTicket(SessionManager.CurrentUserID, movieEvent.ID);

                if (this.XamlRoot?.Content is NavigationPage navPage)
                {
                    navPage.ViewModel.RefreshWallet();
                }
            }
        }
    }
}