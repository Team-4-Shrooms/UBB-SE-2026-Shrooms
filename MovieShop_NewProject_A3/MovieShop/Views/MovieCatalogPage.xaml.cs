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
        : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 29, 185, 84));

    public MovieCatalogItem(Movie movie, int reviewCount)
    {
        Movie = movie;
        ReviewCount = reviewCount;
    }
}

public sealed partial class MovieCatalogPage : Page
{
    private readonly IMovieCatalogService catalogService = App.Services.GetRequiredService<IMovieCatalogService>();
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

        flashSaleVm = SaleService.CurrentSale;
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
        var q = (SearchBox.Text ?? string.Empty).Trim().ToLower();
        var list = string.IsNullOrEmpty(q) ? sourceMovies
                                           : sourceMovies.Where(m => m.Title.ToLower().Contains(q));

        if (SortAscPrice.IsChecked == true)
        {
            list = list.OrderBy(m => m.Price);
        }
        else if (SortDescPrice.IsChecked == true)
        {
            list = list.OrderByDescending(m => m.Price);
        }
        else if (SortHighRating.IsChecked == true)
        {
            list = list.OrderByDescending(m => m.Rating);
        }
        else if (SortLowRating.IsChecked == true)
        {
            list = list.OrderBy(m => m.Rating);
        }
        else
        {
            list = list.OrderBy(m => m.Title);
        }

        MoviesGrid.ItemsSource = list
            .Select(m => new MovieCatalogItem(m, reviewCountByMovieId.TryGetValue(m.ID, out var c) ? c : 0))
            .ToList();
    }

    private void MovieCard_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border)
        {
            return;
        }

        border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 29, 185, 84));
        border.Background = new SolidColorBrush(Color.FromArgb(255, 48, 48, 48));
        border.RenderTransform = new ScaleTransform { ScaleX = 1.03, ScaleY = 1.03 };
    }

    private void MovieCard_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is not Border border)
        {
            return;
        }

        border.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 64, 64));
        border.Background = new SolidColorBrush(Color.FromArgb(255, 42, 42, 42));
        border.RenderTransform = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
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
            MoviesGrid.Opacity = 1.0;
            return;
        }

        MoviesGrid.ItemsSource = null;
        sourceMovies = new List<Movie>();
        FlashSaleEndedText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        MoviesGrid.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    }
}
