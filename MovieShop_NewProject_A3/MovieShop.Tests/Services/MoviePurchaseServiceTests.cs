using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class MoviePurchaseServiceTests
    {
        private readonly Mock<IMovieRepository> mockMovieRepo;
        private readonly Mock<IActiveSalesRepository> mockActiveSalesRepo;
        private readonly MoviePurchaseService service;

        public MoviePurchaseServiceTests()
        {
            mockMovieRepo = new Mock<IMovieRepository>();
            mockActiveSalesRepo = new Mock<IActiveSalesRepository>();
            service = new MoviePurchaseService(mockMovieRepo.Object, mockActiveSalesRepo.Object);
        }

        [Fact]
        public void GetBuyButtonProps_UserOwnsMovie_ReturnsOwnedState()
        {
            // Arrange
            var movie = new Movie { ID = 1, Price = 10 };
            mockMovieRepo.Setup(r => r.UserOwnsMovie(1, 1)).Returns(true);

            // Act
            var props = service.GetBuyButtonProps(movie, 1, true, 100);

            // Assert
            Assert.Equal("Owned", props.Content);
            Assert.False(props.IsEnabled);
            Assert.Null(props.ToolTip);
            Assert.Equal(1.0, props.Opacity);
        }

        [Fact]
        public void GetBuyButtonProps_NotLoggedIn_ReturnsLogInMessage()
        {
            // Arrange
            var movie = new Movie { ID = 1, Price = 10 };
            mockMovieRepo.Setup(r => r.UserOwnsMovie(0, 1)).Returns(false);

            // Act
            var props = service.GetBuyButtonProps(movie, 0, false, 0);

            // Assert
            Assert.Equal("Buy movie", props.Content);
            Assert.False(props.IsEnabled);
            Assert.Contains("must be logged in", props.ToolTip);
            Assert.Equal(0.55, props.Opacity);
        }

        [Fact]
        public void GetBuyButtonProps_LowBalance_ReturnsLowBalanceMessage()
        {
            // Arrange
            var movie = new Movie { ID = 1, Price = 10, ActiveSaleDiscountPercent = 0 }; // Effective price 10
            mockMovieRepo.Setup(r => r.UserOwnsMovie(1, 1)).Returns(false);

            // Act
            var props = service.GetBuyButtonProps(movie, 1, true, 5);

            // Assert
            Assert.Equal("Buy movie", props.Content);
            Assert.False(props.IsEnabled);
            Assert.Contains("balance is too low", props.ToolTip);
            Assert.Equal(0.55, props.Opacity);
        }

        [Fact]
        public void GetBuyButtonProps_ValidPurchase_ReturnsEnabledState()
        {
            // Arrange
            var movie = new Movie { ID = 1, Price = 10, ActiveSaleDiscountPercent = 0 };
            mockMovieRepo.Setup(r => r.UserOwnsMovie(1, 1)).Returns(false);

            // Act
            var props = service.GetBuyButtonProps(movie, 1, true, 20);

            // Assert
            Assert.Equal("Buy movie", props.Content);
            Assert.True(props.IsEnabled);
            Assert.Null(props.ToolTip);
            Assert.Equal(1.0, props.Opacity);
        }

        [Fact]
        public void PurchaseMovie_ValidPurchase_CallsRepository()
        {
            // Arrange
            var movie = new Movie { ID = 1, Price = 10, ActiveSaleDiscountPercent = 0 };

            // Act
            service.PurchaseMovie(1, movie);

            // Assert
            mockMovieRepo.Verify(r => r.PurchaseMovie(1, 1, 10m), Times.Once);
        }
    }
}
