using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MovieShop.Models;
using MovieShop.Services;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

namespace MovieShop.Views
{
    public sealed partial class MainPage : UserControl
    {
        public ViewModels.FlashSaleViewModel? FlashSaleVM => MovieShop.Services.SaleService.CurrentSale;
        private List<Movie> _sourceMovies = new();
        private Dictionary<int, int> _reviewCountByMovieId = new();
        private readonly IMovieCatalogService _catalogService = App.Services.GetRequiredService<IMovieCatalogService>();

        private readonly SolidColorBrush HoverBorderBrush = new SolidColorBrush(Color.FromArgb(255, 29, 185, 84));
        private readonly SolidColorBrush HoverBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 48, 48, 48));
        private readonly SolidColorBrush DefaultBorderBrush = new SolidColorBrush(Color.FromArgb(255, 64, 64, 64));
        private readonly SolidColorBrush DefaultBackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 42, 42, 42));

        public MainPage()
        {
            InitializeComponent();

            FlashSaleVM.PropertyChanged += FlashSaleVM_PropertyChanged!;
            UpdateBigBanner(FlashSaleVM.TimerText, FlashSaleVM.IsActive);

            SearchBox.TextChanged += (_, _) => ApplyFilterAndSort();
            SortAscPrice.IsChecked = true;
            RefreshUndiscountedMovies();
        }

        public void UpdateBigBanner(string time, bool isActive)
        {
            BigTimerText.Text = time;

            if (isActive)
            {
                BigSaleBanner.Visibility = Visibility.Visible;
                BigSaleBanner.Height = double.NaN;
                BigSaleBanner.Margin = new Thickness(0, 0, 0, 20);
            }
            else
            {
                BigSaleBanner.Visibility = Visibility.Collapsed;
                BigSaleBanner.Height = 0;
                BigSaleBanner.Margin = new Thickness(0);
            }
        }

        private void DiscoverButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.XamlRoot.Content is NavigationPage navPage)
            {
                navPage.ViewModel.CurrentViewModel = "SalesPage";
            }
        }

        private void FlashSaleVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateBigBanner(FlashSaleVM.TimerText, FlashSaleVM.IsActive);

                if (e.PropertyName == nameof(ViewModels.FlashSaleViewModel.IsActive))
                    RefreshUndiscountedMovies();
            });
        }

        private void RefreshUndiscountedMovies()
        {
            var (movies, reviewCounts) = _catalogService.GetUndiscountedMovies();

            _sourceMovies = movies;
            _reviewCountByMovieId = reviewCounts;

            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            var searchQuery = (SearchBox.Text ?? "").Trim().ToLower();

            var filteredMovies = string.IsNullOrEmpty(searchQuery)
                ? _sourceMovies
                : _sourceMovies.Where(movie => movie.Title.ToLower().Contains(searchQuery));

            if (SortAscPrice.IsChecked == true) filteredMovies = filteredMovies.OrderBy(movie => movie.Price);
            else if (SortDescPrice.IsChecked == true) filteredMovies = filteredMovies.OrderByDescending(movie => movie.Price);
            else if (SortHighRating.IsChecked == true) filteredMovies = filteredMovies.OrderByDescending(movie => movie.Rating);
            else if (SortLowRating.IsChecked == true) filteredMovies = filteredMovies.OrderBy(movie => movie.Rating);
            else filteredMovies = filteredMovies.OrderBy(movie => movie.Title);

            var orderedMovies = filteredMovies.ToList();
            UndiscountedMoviesGrid.ItemsSource = orderedMovies
                .Select(movie => new MovieCatalogItem(movie, _reviewCountByMovieId.TryGetValue(movie.ID, out var count) ? count : 0))
                .ToList();
        }

        private void SortOption_Changed(object sender, RoutedEventArgs e) => ApplyFilterAndSort();

        private void MovieCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border) return;

            border.BorderBrush = HoverBorderBrush;
            border.Background = HoverBackgroundBrush;
            border.RenderTransform = new ScaleTransform { ScaleX = 1.03, ScaleY = 1.03 };
        }

        private void MovieCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border) return;

            border.BorderBrush = DefaultBorderBrush;
            border.Background = DefaultBackgroundBrush;
            border.RenderTransform = new ScaleTransform { ScaleX = 1, ScaleY = 1 };
        }

        private void UndiscountedMoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not MovieCatalogItem item)
                return;

            NavigateToMovieDetail(item.Movie);
        }

        private void NavigateToMovieDetail(Movie movie)
        {
            var navPage = FindAncestorNavigationPage();
            navPage?.NavigateToMovieDetail(movie, showOnlySales: false);
        }

        private NavigationPage? FindAncestorNavigationPage()
        {
            UIElement? current = this;
            while (current != null)
            {
                if (current is NavigationPage navPage)
                    return navPage;

                current = VisualTreeHelper.GetParent(current) as UIElement;
            }

            return null;
        }
    }
}