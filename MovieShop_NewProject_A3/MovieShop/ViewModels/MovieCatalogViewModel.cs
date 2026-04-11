using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.Views;

namespace MovieShop.ViewModels
{
    public class MovieCatalogViewModel : INotifyPropertyChanged
    {
        private readonly IMovieCatalogService catalogService;
        private readonly ISaleService saleService;

        private List<Movie> sourceMovies = new ();
        private Dictionary<int, int> reviewCountByMovieId = new ();

        private string searchQuery = string.Empty;
        private string sortOption = "PriceAsc";
        private bool showOnlySales;
        private bool isSaleActive;
        private MainViewModel? mainViewModel;

        public MovieCatalogViewModel(IMovieCatalogService catalogService, ISaleService saleService)
        {
            this.catalogService = catalogService;
            this.saleService = saleService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MovieCatalogItem> Items { get; } = new ();

        public MainViewModel? MainViewModel => mainViewModel;

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                if (searchQuery == value)
                {
                    return;
                }

                searchQuery = value ?? string.Empty;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }

        public string SortOption
        {
            get => sortOption;
            set
            {
                if (sortOption == value)
                {
                    return;
                }

                sortOption = value;
                OnPropertyChanged();
                ApplyFilterAndSort();
            }
        }

        public bool IsSaleActive
        {
            get => isSaleActive;
            private set
            {
                if (isSaleActive == value)
                {
                    return;
                }

                isSaleActive = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CatalogVisibility));
                OnPropertyChanged(nameof(SaleEndedVisibility));
            }
        }

        public Visibility CatalogVisibility => IsSaleActive ? Visibility.Visible : Visibility.Collapsed;

        public Visibility SaleEndedVisibility => IsSaleActive ? Visibility.Collapsed : Visibility.Visible;

        public FlashSaleViewModel? CurrentSale => saleService.CurrentSale;

        public void Initialize(MovieCatalogNavArgs? args)
        {
            mainViewModel = args?.MainViewModel;
            showOnlySales = args?.ShowOnlySales ?? false;

            IsSaleActive = saleService.CurrentSale?.IsActive ?? false;
            LoadMovies();
            ApplyFilterAndSort();
        }

        public void RefreshSaleState()
        {
            IsSaleActive = saleService.CurrentSale?.IsActive ?? false;
            LoadMovies();
            ApplyFilterAndSort();
        }

        public Movie? FindMovieById(int id) => sourceMovies.FirstOrDefault(movie => movie.ID == id);

        private void LoadMovies()
        {
            if (!IsSaleActive)
            {
                sourceMovies = new List<Movie>();
                reviewCountByMovieId = new Dictionary<int, int>();
                return;
            }

            var (movies, reviewCounts) = catalogService.GetDiscountedMovies();
            sourceMovies = movies;
            reviewCountByMovieId = reviewCounts;
        }

        private void ApplyFilterAndSort()
        {
            Items.Clear();

            if (!IsSaleActive)
            {
                return;
            }

            var sorted = catalogService.FilterAndSort(sourceMovies, searchQuery, sortOption);
            foreach (var movie in sorted)
            {
                var count = reviewCountByMovieId.TryGetValue(movie.ID, out var value) ? value : 0;
                Items.Add(new MovieCatalogItem(movie, count));
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
