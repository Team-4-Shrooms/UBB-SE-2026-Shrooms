using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using MovieShop.Models;
using MovieShop.ViewModels;
using Windows.UI;

namespace MovieShop.Views
{
    public sealed partial class MainPage : UserControl
    {
        public HomeViewModel ViewModel { get; } = App.Services.GetRequiredService<HomeViewModel>();

        private readonly SolidColorBrush hoverBorderBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.HoverBorderRed, ColorConstants.HoverBorderGreen, ColorConstants.HoverBorderBlue));
        private readonly SolidColorBrush hoverBackgroundBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.HoverBackgroundGray, ColorConstants.HoverBackgroundGray, ColorConstants.HoverBackgroundGray));
        private readonly SolidColorBrush defaultBorderBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.DefaultBorderGray, ColorConstants.DefaultBorderGray, ColorConstants.DefaultBorderGray));
        private readonly SolidColorBrush defaultBackgroundBrush = new SolidColorBrush(Color.FromArgb(ColorConstants.FullAlpha, ColorConstants.DefaultBackgroundGray, ColorConstants.DefaultBackgroundGray, ColorConstants.DefaultBackgroundGray));

        public MainPage()
        {
            InitializeComponent();

            if (ViewModel.FlashSale != null)
            {
                ViewModel.FlashSale.PropertyChanged += FlashSale_PropertyChanged!;
                UpdateBigBanner(ViewModel.FlashSale.TimerText, ViewModel.FlashSale.IsActive);
            }

            SearchBox.TextChanged += (_, _) => ViewModel.SearchQuery = SearchBox.Text ?? string.Empty;
            SortAscPrice.IsChecked = true;
            ViewModel.SortOption = "PriceAsc";
            ViewModel.Load();
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

        private void FlashSale_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var sale = ViewModel.FlashSale;
                if (sale == null)
                {
                    return;
                }

                UpdateBigBanner(sale.TimerText, sale.IsActive);

                if (e.PropertyName == nameof(FlashSaleViewModel.IsActive))
                {
                    ViewModel.Load();
                }
            });
        }

        private void SortOption_Changed(object sender, RoutedEventArgs e)
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
