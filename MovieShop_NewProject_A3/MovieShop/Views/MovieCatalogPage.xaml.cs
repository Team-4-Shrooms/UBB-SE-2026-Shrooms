using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.ViewModels;
using Windows.UI;

namespace MovieShop.Views;

public sealed class MovieCatalogItem
{
    public Movie Movie { get; }
    public int ID => Movie.ID;
    public string Title => Movie.Title;
    public string? ImageUrl => Movie.ImageUrl;
    public double Rating => Movie.Rating;
    public int ReviewCount { get; }

    public string RatingAndReviewCountText => $"Ratings ({ReviewCount}): {Rating:0.0} / 10";

    public bool IsOnSale => Movie.HasActiveSale;

    public string OriginalPriceText => $"$ {Movie.Price:0.00}";

    public string CurrentPriceText => $"$ {Movie.GetEffectivePrice():0.00}";

    public Microsoft.UI.Xaml.Visibility SaleVisibility => IsOnSale
        ? Microsoft.UI.Xaml.Visibility.Visible
        : Microsoft.UI.Xaml.Visibility.Collapsed;

    public Microsoft.UI.Xaml.Media.SolidColorBrush PriceColor => IsOnSale
        ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.IndianRed)
        : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.HoverBorderRed, ColorConstants.HoverBorderGreen, ColorConstants.HoverBorderBlue));

    public MovieCatalogItem(Movie movie, int reviewCount)
    {
        Movie = movie;
        ReviewCount = reviewCount;
    }
}

public sealed partial class MovieCatalogPage : Page
{
    public MovieCatalogViewModel ViewModel { get; } = App.Services.GetRequiredService<MovieCatalogViewModel>();

    public MovieCatalogPage()
    {
        InitializeComponent();
        SearchBox.TextChanged += (_, _) => ViewModel.SearchQuery = SearchBox.Text ?? string.Empty;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        ViewModel.Initialize(e.Parameter as MovieCatalogNavArgs);

        if (ViewModel.CurrentSale != null)
        {
            ViewModel.CurrentSale.PropertyChanged += FlashSaleVm_PropertyChanged!;
        }

        SortAscPrice.IsChecked = true;
        ViewModel.SortOption = "PriceAsc";
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel.CurrentSale != null)
        {
            ViewModel.CurrentSale.PropertyChanged -= FlashSaleVm_PropertyChanged!;
        }
    }

    private void FlashSaleVm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(FlashSaleViewModel.IsActive))
        {
            return;
        }

        DispatcherQueue.TryEnqueue(() => ViewModel.RefreshSaleState());
    }

    private void SortOption_Changed(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (SortAscPrice.IsChecked == true)
        {
            ViewModel.SortOption = "PriceAsc";
        }
        else if (SortDescPrice.IsChecked == true)
        {
            ViewModel.SortOption = "PriceDesc";
        }
        else if (SortHighRating.IsChecked == true)
        {
            ViewModel.SortOption = "RatingHigh";
        }
        else if (SortLowRating.IsChecked == true)
        {
            ViewModel.SortOption = "RatingLow";
        }
    }

    private void MovieCard_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border)
        {
            return;
        }

        border.BorderBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.HoverBorderRed, ColorConstants.HoverBorderGreen, ColorConstants.HoverBorderBlue));
        border.Background = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.HoverBackgroundGray, ColorConstants.HoverBackgroundGray, ColorConstants.HoverBackgroundGray));
        border.RenderTransform = new ScaleTransform { ScaleX = UIConstants.HoverScaleFactor, ScaleY = UIConstants.HoverScaleFactor };
    }

    private void MovieCard_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border)
        {
            return;
        }

        border.BorderBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.DefaultBorderGray, ColorConstants.DefaultBorderGray, ColorConstants.DefaultBorderGray));
        border.Background = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.DefaultBackgroundGray, ColorConstants.DefaultBackgroundGray, ColorConstants.DefaultBackgroundGray));
        border.RenderTransform = new ScaleTransform { ScaleX = UIConstants.DefaultScaleFactor, ScaleY = UIConstants.DefaultScaleFactor };
    }

    private void MoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not MovieCatalogItem item || ViewModel.MainViewModel == null)
        {
            return;
        }

        Frame?.Navigate(typeof(MovieDetailPage), new MovieDetailNavArgs
        {
            Movie = item.Movie,
            MainViewModel = ViewModel.MainViewModel
        });
    }
}
