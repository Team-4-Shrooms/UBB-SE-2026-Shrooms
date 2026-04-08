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
                                     .Select(s => s.Movie.ID)
                                     .Distinct()
                                     .ToHashSet();

            var undiscounted = all
                .Where(m => !onSaleIds.Contains(m.ID))
                .ToList();

            var reviewCounts = reviewService
                .GetReviewCounts(undiscounted.Select(m => m.ID));

            return (undiscounted, reviewCounts);
        }

        public (List<Movie> Movies, Dictionary<int, int> ReviewCounts) GetDiscountedMovies()
        {
            var all = movieRepo.GetAllMovies();

            var discountMap = salesRepo.GetBestDiscountPercentByMovieId();
            ActiveSalesRepo.ApplyBestDiscountsToMovies(all, discountMap);

            var onSaleIds = salesRepo.GetCurrentSales()
                                     .Select(s => s.Movie.ID)
                                     .Distinct()
                                     .ToHashSet();

            var discounted = all
                .Where(m => onSaleIds.Contains(m.ID))
                .ToList();

            var reviewCounts = reviewService
                .GetReviewCounts(discounted.Select(m => m.ID));

            return (discounted, reviewCounts);
        }
    }
}
