using System.Collections.Generic;
using System.Linq;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class MovieCatalogServiceTests
    {
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
            // Arrange
            var movies = new List<Movie>
            {
                new Movie { ID = 1, Title = "Movie A", Price = 10 },
                new Movie { ID = 2, Title = "Movie B", Price = 20 }
            };
            mockMovieRepo.Setup(repo => repo.GetAllMovies()).Returns(movies);
            mockSalesRepo.Setup(repo => repo.GetBestDiscountPercentByMovieId()).Returns(new Dictionary<int, decimal>());
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            mockReviewService.Setup(service => service.GetReviewCounts(It.IsAny<IEnumerable<int>>())).Returns(new Dictionary<int, int>());

            // Act
            var (resultMovies, reviewCounts) = service.GetUndiscountedMovies();

            // Assert
            Assert.Equal(2, resultMovies.Count);
        }

        [Fact]
        public void GetDiscountedMovies_WithSales_ReturnsOnlySaleMovies()
        {
            // Arrange
            var movies = new List<Movie>
            {
                new Movie { ID = 1, Title = "Movie A", Price = 10 },
                new Movie { ID = 2, Title = "Movie B", Price = 20 }
            };
            var sales = new List<ActiveSale>
            {
                new ActiveSale { Movie = new Movie { ID = 1 } }
            };
            mockMovieRepo.Setup(repo => repo.GetAllMovies()).Returns(movies);
            mockSalesRepo.Setup(repo => repo.GetBestDiscountPercentByMovieId()).Returns(new Dictionary<int, decimal> { { 1, 20 } });
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(sales);
            mockReviewService.Setup(service => service.GetReviewCounts(It.IsAny<IEnumerable<int>>())).Returns(new Dictionary<int, int>());

            // Act
            var (resultMovies, reviewCounts) = service.GetDiscountedMovies();

            // Assert
            Assert.Single(resultMovies);
            Assert.Equal(1, resultMovies[0].ID);
        }

        [Fact]
        public void FilterAndSort_EmptySearch_ReturnsAllMovies()
        {
            // Arrange
            var movies = new List<Movie>
            {
                new Movie { ID = 1, Title = "Zebra", Price = 30 },
                new Movie { ID = 2, Title = "Alpha", Price = 10 }
            };

            // Act
            var result = service.FilterAndSort(movies, string.Empty, "Title");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Alpha", result[0].Title);
        }

        [Fact]
        public void FilterAndSort_WithSearch_FiltersCorrectly()
        {
            // Arrange
            var movies = new List<Movie>
            {
                new Movie { ID = 1, Title = "Action Movie", Price = 10 },
                new Movie { ID = 2, Title = "Comedy Film", Price = 20 }
            };

            // Act
            var result = service.FilterAndSort(movies, "action", "Title");

            // Assert
            Assert.Single(result);
            Assert.Equal("Action Movie", result[0].Title);
        }

        [Theory]
        [InlineData("PriceAsc", "Cheap")]
        [InlineData("PriceDesc", "Expensive")]
        [InlineData("RatingHigh", "Expensive")]
        [InlineData("RatingLow", "Cheap")]
        public void FilterAndSort_SortOptions_SortsCorrectly(string sortOption, string expectedFirstTitle)
        {
            // Arrange
            var movies = new List<Movie>
            {
                new Movie { ID = 1, Title = "Expensive", Price = 30, Rating = 9.0 },
                new Movie { ID = 2, Title = "Cheap", Price = 5, Rating = 2.0 }
            };

            // Act
            var result = service.FilterAndSort(movies, string.Empty, sortOption);

            // Assert
            Assert.Equal(expectedFirstTitle, result[0].Title);
        }
    }
}
