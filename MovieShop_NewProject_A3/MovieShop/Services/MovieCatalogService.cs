using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class MovieCatalogService : IMovieCatalogService
    {
        private readonly IMovieRepository movieRepo;
        private readonly IActiveSalesRepository salesRepo;
        private readonly IMovieReviewService reviewService;
        public MovieCatalogService(IMovieRepository movieRepo, IActiveSalesRepository activeSalesRepo, IMovieReviewService reviewService)
        {
            this.movieRepo = movieRepo;
            salesRepo = activeSalesRepo;
            this.reviewService = reviewService;
        }
        public void ApplyDiscount(Movie movie)
        {
            var discountMap = salesRepo.GetBestDiscountPercentByMovieId();
            ActiveSalesRepo.ApplyBestDiscountsToMovies(new List<Movie> { movie }, discountMap);
        }

        public (List<Movie> Movies, Dictionary<int, int> ReviewCounts) GetUndiscountedMovies()
        {
            var all = movieRepo.GetAllMovies();

            var discountMap = salesRepo.GetBestDiscountPercentByMovieId();
            ActiveSalesRepo.ApplyBestDiscountsToMovies(all, discountMap);

            var onSaleIds = salesRepo.GetCurrentSales()
                                     .Select(sale => sale.Movie.ID)
                                     .Distinct()
                                     .ToHashSet();

            var undiscounted = all
                .Where(movie => !onSaleIds.Contains(movie.ID))
                .ToList();

            var reviewCounts = reviewService
                .GetReviewCounts(undiscounted.Select(movie => movie.ID));

            return (undiscounted, reviewCounts);
        }

        public (List<Movie> Movies, Dictionary<int, int> ReviewCounts) GetDiscountedMovies()
        {
            var all = movieRepo.GetAllMovies();

            var discountMap = salesRepo.GetBestDiscountPercentByMovieId();
            ActiveSalesRepo.ApplyBestDiscountsToMovies(all, discountMap);

            var onSaleIds = salesRepo.GetCurrentSales()
                                     .Select(sale => sale.Movie.ID)
                                     .Distinct()
                                     .ToHashSet();

            var discounted = all
                .Where(movie => onSaleIds.Contains(movie.ID))
                .ToList();

            var reviewCounts = reviewService
                .GetReviewCounts(discounted.Select(movie => movie.ID));

            return (discounted, reviewCounts);
        }

        public List<Movie> FilterAndSort(List<Movie> movies, string searchQuery, string sortOption)
        {
            var filtered = string.IsNullOrEmpty(searchQuery)
                ? movies.AsEnumerable()
                : movies.Where(movie => movie.Title.ToLower().Contains(searchQuery.ToLower()));

            filtered = sortOption switch
            {
                "PriceAsc" => filtered.OrderBy(movie => movie.Price),
                "PriceDesc" => filtered.OrderByDescending(movie => movie.Price),
                "RatingHigh" => filtered.OrderByDescending(movie => movie.Rating),
                "RatingLow" => filtered.OrderBy(movie => movie.Rating),
                _ => filtered.OrderBy(movie => movie.Title),
            };

            return filtered.ToList();
        }
    }
}
