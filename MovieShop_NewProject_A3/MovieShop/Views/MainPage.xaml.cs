using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MovieShop.Models;
using MovieShop.Services;
using Windows.UI;

namespace MovieShop.Views
{
    public sealed partial class MainPage : UserControl
    {
        private readonly ISaleService saleService = App.Services.GetRequiredService<ISaleService>();
        public ViewModels.FlashSaleViewModel? FlashSaleVM => saleService.CurrentSale;
        private List<Movie> sourceMovies = new ();
        private Dictionary<int, int> reviewCountByMovieId = new ();
        private readonly IMovieCatalogService catalogService = App.Services.GetRequiredService<IMovieCatalogService>();

        private readonly SolidColorBrush hoverBorderBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.HoverBorderRed, ColorConstants.HoverBorderGreen, ColorConstants.HoverBorderBlue));
        private readonly SolidColorBrush hoverBackgroundBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.HoverBackgroundGray, ColorConstants.HoverBackgroundGray, ColorConstants.HoverBackgroundGray));
        private readonly SolidColorBrush defaultBorderBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.DefaultBorderGray, ColorConstants.DefaultBorderGray, ColorConstants.DefaultBorderGray));
        private readonly SolidColorBrush defaultBackgroundBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.DefaultBackgroundGray, ColorConstants.DefaultBackgroundGray, ColorConstants.DefaultBackgroundGray));

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
                BigSaleBanner.Margin = new Thickness(0, 0, 0, UIConstants.BannerBottomMargin);
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
                {
                    RefreshUndiscountedMovies();
                }
            });
        }

        private void RefreshUndiscountedMovies()
        {
            var (movies, reviewCounts) = catalogService.GetUndiscountedMovies();

            sourceMovies = movies;
            reviewCountByMovieId = reviewCounts;

            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            var searchQuery = (SearchBox.Text ?? string.Empty).Trim();
            var sortOption = GetSelectedSortOption();

            var sortedMovies = catalogService.FilterAndSort(sourceMovies, searchQuery, sortOption);
            UndiscountedMoviesGrid.ItemsSource = sortedMovies
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

        private void SortOption_Changed(object sender, RoutedEventArgs e) => ApplyFilterAndSort();

        private void MovieCard_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }

            border.BorderBrush = hoverBorderBrush;
            border.Background = hoverBackgroundBrush;
            border.RenderTransform = new ScaleTransform { ScaleX = UIConstants.HoverScaleFactor, ScaleY = UIConstants.HoverScaleFactor };
        }

        private void MovieCard_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not Border border)
            {
                return;
            }

            border.BorderBrush = defaultBorderBrush;
            border.Background = defaultBackgroundBrush;
            border.RenderTransform = new ScaleTransform { ScaleX = UIConstants.DefaultScaleFactor, ScaleY = UIConstants.DefaultScaleFactor };
        }

        private void UndiscountedMoviesGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is not MovieCatalogItem item)
            {
                return;
            }

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
                {
                    return navPage;
                }

                current = VisualTreeHelper.GetParent(current) as UIElement;
            }

            return null;
        }
    }
}