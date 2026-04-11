using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.Views;

namespace MovieShop.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        private readonly IMovieCatalogService catalogService;
        private readonly ISaleService saleService;

        private List<Movie> sourceMovies = new ();
        private Dictionary<int, int> reviewCountByMovieId = new ();

        private string searchQuery = string.Empty;
        private string sortOption = "PriceAsc";

        public HomeViewModel(IMovieCatalogService catalogService, ISaleService saleService)
        {
            this.catalogService = catalogService;
            this.saleService = saleService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MovieCatalogItem> Items { get; } = new ();

        public FlashSaleViewModel? FlashSale => saleService.CurrentSale;

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

        public void Load()
        {
            var (movies, reviewCounts) = catalogService.GetUndiscountedMovies();
            sourceMovies = movies;
            reviewCountByMovieId = reviewCounts;
            ApplyFilterAndSort();
        }

        private void ApplyFilterAndSort()
        {
            Items.Clear();

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
