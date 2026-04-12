using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class MovieCatalogServiceTests
    {
        private const int MovieIdA = 1;
        private const int MovieIdB = 2;
        private const decimal PriceA = 10m;
        private const decimal PriceB = 20m;
        private const decimal DiscountPercent = 20m;
        private const decimal ExpensivePrice = 30m;
        private const decimal CheapPrice = 5m;
        private const double HighRating = 9.0;
        private const double LowRating = 2.0;

        private readonly Mock<IMovieRepository> mockMovieRepo;
        private readonly Mock<IActiveSalesRepository> mockSalesRepo;
        private readonly Mock<IMovieReviewService> mockReviewService;
        private readonly MovieCatalogService service;

        public MovieCatalogServiceTests()
        {
            mockMovieRepo = new Mock<IMovieRepository>();
            mockSalesRepo = new Mock<IActiveSalesRepository>();
            mockReviewService = new Mock<IMovieReviewService>();
            service = new MovieCatalogService(mockMovieRepo.Object, mockSalesRepo.Object, mockReviewService.Object);
        }

        [Fact]
        public void GetUndiscountedMovies_NoSales_ReturnsAllMovies()
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Movie A", Price = PriceA },
                new Movie { ID = MovieIdB, Title = "Movie B", Price = PriceB },
            };
            mockMovieRepo.Setup(repo => repo.GetAllMovies()).Returns(movies);
            mockSalesRepo.Setup(repo => repo.GetBestDiscountPercentByMovieId()).Returns(new Dictionary<int, decimal>());
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            mockReviewService.Setup(service => service.GetReviewCounts(It.IsAny<IEnumerable<int>>())).Returns(new Dictionary<int, int>());

            var (resultMovies, reviewCounts) = service.GetUndiscountedMovies();

            Assert.Equal(2, resultMovies.Count);
        }

        [Fact]
        public void GetDiscountedMovies_WithSales_ReturnsOnlySaleMovies()
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Movie A", Price = PriceA },
                new Movie { ID = MovieIdB, Title = "Movie B", Price = PriceB },
            };
            var sales = new List<ActiveSale>
            {
                new ActiveSale { Movie = new Movie { ID = MovieIdA } },
            };
            mockMovieRepo.Setup(repo => repo.GetAllMovies()).Returns(movies);
            mockSalesRepo.Setup(repo => repo.GetBestDiscountPercentByMovieId()).Returns(new Dictionary<int, decimal> { { MovieIdA, DiscountPercent } });
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(sales);
            mockReviewService.Setup(service => service.GetReviewCounts(It.IsAny<IEnumerable<int>>())).Returns(new Dictionary<int, int>());

            var (resultMovies, reviewCounts) = service.GetDiscountedMovies();

            Assert.Single(resultMovies);
        }

        [Fact]
        public void GetDiscountedMovies_WithSales_ReturnsCorrectMovie()
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Movie A", Price = PriceA },
                new Movie { ID = MovieIdB, Title = "Movie B", Price = PriceB },
            };
            var sales = new List<ActiveSale>
            {
                new ActiveSale { Movie = new Movie { ID = MovieIdA } },
            };
            mockMovieRepo.Setup(repo => repo.GetAllMovies()).Returns(movies);
            mockSalesRepo.Setup(repo => repo.GetBestDiscountPercentByMovieId()).Returns(new Dictionary<int, decimal> { { MovieIdA, DiscountPercent } });
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(sales);
            mockReviewService.Setup(service => service.GetReviewCounts(It.IsAny<IEnumerable<int>>())).Returns(new Dictionary<int, int>());

            var (resultMovies, reviewCounts) = service.GetDiscountedMovies();

            Assert.Equal(MovieIdA, resultMovies[0].ID);
        }

        [Fact]
        public void FilterAndSort_EmptySearch_ReturnsAllMovies()
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Zebra", Price = ExpensivePrice },
                new Movie { ID = MovieIdB, Title = "Alpha", Price = PriceA },
            };

            var result = service.FilterAndSort(movies, string.Empty, "Title");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FilterAndSort_EmptySearch_SortsByTitle()
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Zebra", Price = ExpensivePrice },
                new Movie { ID = MovieIdB, Title = "Alpha", Price = PriceA },
            };

            var result = service.FilterAndSort(movies, string.Empty, "Title");

            Assert.Equal("Alpha", result[0].Title);
        }

        [Fact]
        public void FilterAndSort_WithSearch_ReturnsMatchingCount()
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Action Movie", Price = PriceA },
                new Movie { ID = MovieIdB, Title = "Comedy Film", Price = PriceB },
            };

            var result = service.FilterAndSort(movies, "action", "Title");

            Assert.Single(result);
        }

        [Fact]
        public void FilterAndSort_WithSearch_ReturnsCorrectMovie()
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Action Movie", Price = PriceA },
                new Movie { ID = MovieIdB, Title = "Comedy Film", Price = PriceB },
            };

            var result = service.FilterAndSort(movies, "action", "Title");

            Assert.Equal("Action Movie", result[0].Title);
        }

        [Theory]
        [InlineData("PriceAsc", "Cheap")]
        [InlineData("PriceDesc", "Expensive")]
        [InlineData("RatingHigh", "Expensive")]
        [InlineData("RatingLow", "Cheap")]
        public void FilterAndSort_SortOptions_SortsCorrectly(string sortOption, string expectedFirstTitle)
        {
            var movies = new List<Movie>
            {
                new Movie { ID = MovieIdA, Title = "Expensive", Price = ExpensivePrice, Rating = HighRating },
                new Movie { ID = MovieIdB, Title = "Cheap", Price = CheapPrice, Rating = LowRating },
            };

            var result = service.FilterAndSort(movies, string.Empty, sortOption);

            Assert.Equal(expectedFirstTitle, result[0].Title);
        }
    }
}
