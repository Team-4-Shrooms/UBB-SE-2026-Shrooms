using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.ViewModels;

namespace MovieShop.Views;

public sealed partial class MovieDetailPage : Page
{
    public MovieDetailViewModel ViewModel { get; } = App.Services.GetRequiredService<MovieDetailViewModel>();

    public MovieDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Initialize(e.Parameter as MovieDetailNavArgs);
        TrySetPoster(ViewModel.PosterUrl);
    }

    private void TrySetPoster(string? url)
    {
        PosterImage.Source = null;
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            PosterImage.Source = new BitmapImage(new Uri(url, UriKind.Absolute));
        }
        catch (UriFormatException)
        {
        }
    }

    private async void BuyMovieButton_Click(object sender, RoutedEventArgs e)
    {
        var movie = ViewModel.Movie;
        if (movie == null)
        {
            return;
        }

        ViewModel.RefreshBuyButtonState();
        if (!ViewModel.CanBuyMovie)
        {
            return;
        }

        var confirmDialog = new ContentDialog
        {
            Title = "Confirm purchase",
            Content = $"Buy \"{movie.Title}\" for {movie.GetEffectivePrice():C}? This will be charged to your balance.",
            PrimaryButtonText = "Buy",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };

        if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        if (!ViewModel.TryPurchase(out var error))
        {
            var errorDialog = new ContentDialog
            {
                Title = "Cannot complete purchase",
                Content = error,
                PrimaryButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            _ = await errorDialog.ShowAsync();
            return;
        }

        var successDialog = new ContentDialog
        {
            Title = "Purchase successful",
            Content = $"You now own \"{movie.Title}\". It has been added to your inventory.",
            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };
        _ = await successDialog.ShowAsync();

        if (this.XamlRoot?.Content is NavigationPage navPage)
        {
            navPage.ViewModel.CurrentViewModel = "Inventory";
        }
    }

    private void ReviewsButton_Click(object sender, RoutedEventArgs e)
    {
        var movie = ViewModel.Movie;
        var mainViewModel = ViewModel.MainViewModel;
        if (movie == null || mainViewModel == null)
        {
            return;
        }

        Frame?.Navigate(typeof(MovieReviewsPage), new MovieReviewsNavArgs
        {
            Movie = movie,
            MainViewModel = mainViewModel,
        });
    }

    private void EventsButton_Click(object sender, RoutedEventArgs e)
    {
        var movie = ViewModel.Movie;
        var mainViewModel = ViewModel.MainViewModel;
        if (movie == null || mainViewModel == null)
        {
            return;
        }

        Frame?.Navigate(typeof(MovieEventsPage), new MovieEventsNavArgs
        {
            Movie = movie,
            MainViewModel = mainViewModel,
        });
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
            return;
        }

        if (this.XamlRoot?.Content is NavigationPage navPage)
        {
            navPage.ViewModel.CurrentViewModel = "Shop";
        }
    }
}
