using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Views;

public sealed partial class MovieDetailPage : Page
{
    private const double MaxRating = 10.0;

    private Movie? movie;
    private MainViewModel? mainViewModel;
    private readonly IMoviePurchaseService purchaseService = App.Services.GetRequiredService<IMoviePurchaseService>();
    private readonly IMovieReviewService reviewService = App.Services.GetRequiredService<IMovieReviewService>();
    private readonly IMovieCatalogService movieCatalogService = App.Services.GetRequiredService<IMovieCatalogService>();

    public MovieDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MovieDetailNavArgs args)
        {
            return;
        }

        movie = args.Movie;
        mainViewModel = args.MainViewModel;

        if (movie == null)
        {
            return;
        }

        movieCatalogService.ApplyDiscount(movie);

        TitleBlock.Text = movie.Title;
        DescriptionBlock.Text = string.IsNullOrEmpty(movie.Description) ? "—" : movie.Description;

        RatingBlock.Text = $"Rating: {movie.Rating:0.0} / {MaxRating}";

        UpdatePriceDisplay();

        TrySetPoster(movie.ImageUrl);

        RefreshBuyButtonState();
        ToolTipService.SetToolTip(
            ReviewsButton,
            reviewService.BuildStarDistributionTooltip(movie.ID));
    }

    private void UpdatePriceDisplay()
    {
        if (movie == null)
        {
            return;
        }

        PriceBlock.Text = $"${movie.DiscountedPriceText}";

        if (movie.HasActiveSale)
        {
            OriginalPriceBlock.Visibility = Visibility.Visible;
            OriginalPriceBlock.Text = $"${movie.OriginalPriceText}";
        }
        else
        {
            OriginalPriceBlock.Visibility = Visibility.Collapsed;
        }
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

    private void RefreshBuyButtonState()
    {
        if (movie == null)
        {
            return;
        }

        mainViewModel?.RefreshBalanceFromDatabase();
        var buttonProperties = purchaseService.GetBuyButtonProps(
            movie,
            SessionManager.CurrentUserID,
            SessionManager.IsLoggedIn,
            mainViewModel?.Balance ?? SessionManager.CurrentUserBalance);

        BuyMovieButton.Content = buttonProperties.Content;
        BuyMovieButton.IsEnabled = buttonProperties.IsEnabled;
        BuyMovieButton.Opacity = buttonProperties.Opacity;
        ToolTipService.SetToolTip(BuyMovieButton, buttonProperties.ToolTip);
    }

    private async void BuyMovieButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (movie == null || mainViewModel == null)
        {
            return;
        }

        if (!SessionManager.IsLoggedIn)
        {
            return;
        }

        RefreshBuyButtonState();
        if (!BuyMovieButton.IsEnabled)
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
            XamlRoot = XamlRoot
        };

        if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        try
        {
            purchaseService.PurchaseMovie(SessionManager.CurrentUserID, movie);

            mainViewModel.RefreshWallet();
            SessionManager.CurrentUserBalance = mainViewModel.Balance;

            RefreshBuyButtonState();

            var successDialog = new ContentDialog
            {
                Title = "Purchase successful",
                Content = $"You now own \"{movie.Title}\". It has been added to your inventory.",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = XamlRoot
            };
            _ = await successDialog.ShowAsync();

            if (this.XamlRoot?.Content is NavigationPage navPage)
            {
                navPage.ViewModel.CurrentViewModel = "Inventory";
            }
        }
        catch (InvalidOperationException exception)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Cannot complete purchase",
                Content = exception.Message,
                PrimaryButtonText = "OK",
                XamlRoot = XamlRoot
            };
            _ = await errorDialog.ShowAsync();
        }
    }

    private void ReviewsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (movie == null || mainViewModel == null)
        {
            return;
        }

        Frame?.Navigate(typeof(MovieReviewsPage), new MovieReviewsNavArgs
        {
            Movie = movie,
            MainViewModel = mainViewModel
        });
    }

    private void EventsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (movie == null || mainViewModel == null)
        {
            return;
        }

        Frame?.Navigate(typeof(MovieEventsPage), new MovieEventsNavArgs
        {
            Movie = movie,
            MainViewModel = mainViewModel
        });
    }

    private void BackButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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