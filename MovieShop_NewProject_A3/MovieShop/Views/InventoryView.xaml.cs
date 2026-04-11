using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.ViewModels;

namespace MovieShop.Views
{
    public sealed partial class InventoryView : Page
    {
        public InventoryViewModel ViewModel { get; } = App.Services.GetRequiredService<InventoryViewModel>();

        public InventoryView()
        {
            InitializeComponent();
            Loaded += (_, _) => ViewModel.Load();
        }

        private void MoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Movie movie)
            {
                Frame?.Navigate(typeof(MovieDetailPage), new MovieDetailNavArgs { Movie = movie, MainViewModel = null! });
            }
        }

        private async void RemoveMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.DataContext is not Movie movie)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Remove movie",
                Content = $"Are you sure you want to remove '{movie.Title}' from your library? This will allow you to purchase it again.",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot,
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            ViewModel.RemoveMovie(movie);

            if (XamlRoot?.Content is NavigationPage navPage)
            {
                navPage.ViewModel.RefreshWallet();
            }
        }

        private async void RemoveTicket_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.DataContext is not MovieEvent movieEvent)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                Title = "Remove ticket",
                Content = $"Remove your ticket for '{movieEvent.Title}'? This will allow you to buy it again.",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot,
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            ViewModel.RemoveTicket(movieEvent);

            if (XamlRoot?.Content is NavigationPage navPage)
            {
                navPage.ViewModel.RefreshWallet();
            }
        }
    }
}
