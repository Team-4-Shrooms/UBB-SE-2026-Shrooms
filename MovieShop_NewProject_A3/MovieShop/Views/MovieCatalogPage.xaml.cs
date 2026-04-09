using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Services;
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
    private readonly IMovieCatalogService catalogService = App.Services.GetRequiredService<IMovieCatalogService>();
    private readonly ISaleService saleService = App.Services.GetRequiredService<ISaleService>();
    private List<Movie> sourceMovies = new ();
    private Dictionary<int, int> reviewCountByMovieId = new ();
    private MainViewModel? mainVm;
    private bool showOnlySales;
    private FlashSaleViewModel? flashSaleVm;

    public MovieCatalogPage()
    {
        InitializeComponent();
        SearchBox.TextChanged += (_, _) => ApplyFilterAndSort();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is MovieCatalogNavArgs args)
        {
            mainVm = args.MainViewModel;
            showOnlySales = args.ShowOnlySales;
        }

        LoadDiscountedMovies();

        if (flashSaleVm != null)
        {
            flashSaleVm.PropertyChanged -= FlashSaleVm_PropertyChanged!;
        }

        flashSaleVm = saleService.CurrentSale;
        if (flashSaleVm != null)
        {
            flashSaleVm.PropertyChanged += FlashSaleVm_PropertyChanged!;
        }

        ApplyCatalogDeactivation(flashSaleVm?.IsActive ?? false);

        SortAscPrice.IsChecked = true;
        ApplyFilterAndSort();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (flashSaleVm != null)
        {
            flashSaleVm.PropertyChanged -= FlashSaleVm_PropertyChanged!;
        }
    }

    private void SortOption_Changed(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) => ApplyFilterAndSort();

    private void ApplyFilterAndSort()
    {
        var searchQuery = (SearchBox.Text ?? string.Empty).Trim();
        var sortOption = GetSelectedSortOption();

        var sortedMovies = catalogService.FilterAndSort(sourceMovies, searchQuery, sortOption);
        MoviesGrid.ItemsSource = sortedMovies
            .Select(movie => new MovieCatalogItem(movie, reviewCountByMovieId.TryGetValue(movie.ID, out var count) ? count : 0))
            .ToList();
    }

    private string GetSelectedSortOption()
    {
        if (SortAscPrice.IsChecked == true)
        {
            return "PriceAsc";
        }

        if (SortDescPrice.IsChecked == true)
        {
            return "PriceDesc";
        }

        if (SortHighRating.IsChecked == true)
        {
            return "RatingHigh";
        }

        if (SortLowRating.IsChecked == true)
        {
            return "RatingLow";
        }

        return "Title";
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
        if (e.ClickedItem is not MovieCatalogItem item || mainVm == null)
        {
            return;
        }

        Frame?.Navigate(typeof(MovieDetailPage), new MovieDetailNavArgs
        {
            Movie = item.Movie,
            MainViewModel = mainVm
        });
    }

    private void LoadDiscountedMovies()
    {
        var (movies, reviewCounts) = catalogService.GetDiscountedMovies();
        sourceMovies = movies;
        reviewCountByMovieId = reviewCounts;
    }

    private void FlashSaleVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(FlashSaleViewModel.IsActive))
        {
            return;
        }

        DispatcherQueue.TryEnqueue(() =>
        {
            ApplyCatalogDeactivation(flashSaleVm?.IsActive ?? false);
            if (flashSaleVm?.IsActive ?? false)
            {
                LoadDiscountedMovies();
                ApplyFilterAndSort();
            }
        });
    }

    private void ApplyCatalogDeactivation(bool isSaleActive)
    {
        if (isSaleActive)
        {
            FlashSaleEndedText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            MoviesGrid.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            MoviesGrid.IsEnabled = true;
            MoviesGrid.Opacity = UIConstants.EnabledButtonOpacity;
            return;
        }

        MoviesGrid.ItemsSource = null;
        sourceMovies = new List<Movie>();
        FlashSaleEndedText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        MoviesGrid.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }
}
