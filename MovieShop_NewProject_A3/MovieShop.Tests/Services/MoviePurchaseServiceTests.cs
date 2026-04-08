using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class MoviePurchaseServiceTests
    {
        private readonly Mock<IMovieRepository> _mockMovieRepo;
        private readonly Mock<IActiveSalesRepository> _mockActiveSalesRepo;
        private readonly MoviePurchaseService _service;

        public MoviePurchaseServiceTests()
        {
            _mockMovieRepo = new Mock<IMovieRepository>();
            _mockActiveSalesRepo = new Mock<IActiveSalesRepository>();
            _service = new MoviePurchaseService(_mockMovieRepo.Object, _mockActiveSalesRepo.Object);
        }

        [Fact]
        public void GetBuyButtonProps_UserOwnsMovie_ReturnsOwnedState()
        {
            // Arrange
            var movie = new Movie { ID = 1, Price = 10 };
            _mockMovieRepo.Setup(r => r.UserOwnsMovie(1, 1)).Returns(true);

            // Act
            var props = _service.GetBuyButtonProps(movie, 1, true, 100);

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
            _mockMovieRepo.Setup(r => r.UserOwnsMovie(0, 1)).Returns(false);

            // Act
            var props = _service.GetBuyButtonProps(movie, 0, false, 0);

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
            _mockMovieRepo.Setup(r => r.UserOwnsMovie(1, 1)).Returns(false);

            // Act
            var props = _service.GetBuyButtonProps(movie, 1, true, 5);

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
            _mockMovieRepo.Setup(r => r.UserOwnsMovie(1, 1)).Returns(false);

            // Act
            var props = _service.GetBuyButtonProps(movie, 1, true, 20);

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
            _service.PurchaseMovie(1, movie);

            // Assert
            _mockMovieRepo.Verify(r => r.PurchaseMovie(1, 1, 10m), Times.Once);
        }
    }
}
