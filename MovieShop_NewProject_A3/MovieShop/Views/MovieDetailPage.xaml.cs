using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;
using System;

namespace MovieShop.Views;

public sealed partial class MovieDetailPage : Page
{
    private const double MaxRating = 10.0; 

    private Movie? _movie;
    private MainViewModel? _mainViewModel;
    private readonly IMoviePurchaseService _purchaseService = App.Services.GetRequiredService<IMoviePurchaseService>();
    private readonly IMovieReviewService _reviewService = App.Services.GetRequiredService<IMovieReviewService>();
    private readonly IMovieCatalogService _movieCatalogService = App.Services.GetRequiredService<IMovieCatalogService>();

    public MovieDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MovieDetailNavArgs args)
            return;

        _movie = args.Movie;
        _mainViewModel = args.MainViewModel;

        if (_movie == null)
            return;

        _movieCatalogService.ApplyDiscount(_movie);

        TitleBlock.Text = _movie.Title;
        DescriptionBlock.Text = string.IsNullOrEmpty(_movie.Description) ? "—" : _movie.Description;

        RatingBlock.Text = $"Rating: {_movie.Rating:0.0} / {MaxRating}";

        UpdatePriceDisplay();

        TrySetPoster(_movie.ImageUrl);

        RefreshBuyButtonState();
        ToolTipService.SetToolTip(
            ReviewsButton,
            _reviewService.BuildStarDistributionTooltip(_movie.ID)
        );
    }

    private void UpdatePriceDisplay()
    {
        if (_movie == null)
            return;

        PriceBlock.Text = $"${_movie.DiscountedPriceText}";

        if (_movie.HasActiveSale)
        {
            OriginalPriceBlock.Visibility = Visibility.Visible;
            OriginalPriceBlock.Text = $"${_movie.OriginalPriceText}";
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
            return;
        try
        {
            PosterImage.Source = new BitmapImage(new Uri(url, UriKind.Absolute));
        }
        catch
        {
            /* ignore invalid image URL */
        }
    }

    private void RefreshBuyButtonState()
    {
        if (_movie == null)
            return;

        _mainViewModel?.RefreshBalanceFromDatabase();
        var buttonProperties = _purchaseService.GetBuyButtonProps( 
            _movie,
            SessionManager.CurrentUserID,
            SessionManager.IsLoggedIn,
            _mainViewModel?.Balance ?? SessionManager.CurrentUserBalance
        );

        BuyMovieButton.Content = buttonProperties.Content;
        BuyMovieButton.IsEnabled = buttonProperties.IsEnabled;
        BuyMovieButton.Opacity = buttonProperties.Opacity;
        ToolTipService.SetToolTip(BuyMovieButton, buttonProperties.ToolTip);
    }

    private async void BuyMovieButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_movie == null || _mainViewModel == null)
            return;

        if (!SessionManager.IsLoggedIn)
            return;

        RefreshBuyButtonState();
        if (!BuyMovieButton.IsEnabled)
            return;

        var confirmDialog = new ContentDialog 
        {
            Title = "Confirm purchase",
            Content = $"Buy \"{_movie.Title}\" for {_movie.GetEffectivePrice():C}? This will be charged to your balance.",
            PrimaryButtonText = "Buy",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        if (await confirmDialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        try
        {
            _purchaseService.PurchaseMovie(SessionManager.CurrentUserID, _movie);

            _mainViewModel.RefreshWallet();
            SessionManager.CurrentUserBalance = _mainViewModel.Balance;

            RefreshBuyButtonState();

            var successDialog = new ContentDialog 
            {
                Title = "Purchase successful",
                Content = $"You now own \"{_movie.Title}\". It has been added to your inventory.",
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
        if (_movie == null || _mainViewModel == null)
            return;

        Frame?.Navigate(typeof(MovieReviewsPage), new MovieReviewsNavArgs
        {
            Movie = _movie,
            MainViewModel = _mainViewModel
        });
    }

    private void EventsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_movie == null || _mainViewModel == null)
            return;

        Frame?.Navigate(typeof(MovieEventsPage), new MovieEventsNavArgs
        {
            Movie = _movie,
            MainViewModel = _mainViewModel
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