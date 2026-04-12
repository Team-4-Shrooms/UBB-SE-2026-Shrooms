using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class MoviePurchaseServiceTests
    {
        private const int ValidUserId = 1;
        private const int NotLoggedInUserId = 0;
        private const int ValidMovieId = 1;
        private const decimal MoviePrice = 10m;
        private const decimal HighBalance = 100m;
        private const decimal SufficientBalance = 20m;
        private const decimal InsufficientBalance = 5m;
        private const decimal NoDiscount = 0m;
        private const double FullOpacity = 1.0;
        private const double DisabledOpacity = 0.55;

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
        public void GetBuyButtonProps_UserOwnsMovie_ReturnsOwnedContent()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(true);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, HighBalance);

            Assert.Equal("Owned", props.Content);
        }

        [Fact]
        public void GetBuyButtonProps_UserOwnsMovie_ReturnsDisabled()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(true);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, HighBalance);

            Assert.False(props.IsEnabled);
        }

        [Fact]
        public void GetBuyButtonProps_UserOwnsMovie_ReturnsNullToolTip()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(true);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, HighBalance);

            Assert.Null(props.ToolTip);
        }

        [Fact]
        public void GetBuyButtonProps_UserOwnsMovie_ReturnsFullOpacity()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(true);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, HighBalance);

            Assert.Equal(FullOpacity, props.Opacity);
        }

        [Fact]
        public void GetBuyButtonProps_NotLoggedIn_ReturnsBuyMovieContent()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(NotLoggedInUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, NotLoggedInUserId, false, 0);

            Assert.Equal("Buy movie", props.Content);
        }

        [Fact]
        public void GetBuyButtonProps_NotLoggedIn_ReturnsDisabled()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(NotLoggedInUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, NotLoggedInUserId, false, 0);

            Assert.False(props.IsEnabled);
        }

        [Fact]
        public void GetBuyButtonProps_NotLoggedIn_ReturnsLoginToolTip()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(NotLoggedInUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, NotLoggedInUserId, false, 0);

            Assert.Contains("must be logged in", props.ToolTip);
        }

        [Fact]
        public void GetBuyButtonProps_NotLoggedIn_ReturnsReducedOpacity()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(NotLoggedInUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, NotLoggedInUserId, false, 0);

            Assert.Equal(DisabledOpacity, props.Opacity);
        }

        [Fact]
        public void GetBuyButtonProps_LowBalance_ReturnsBuyMovieContent()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, InsufficientBalance);

            Assert.Equal("Buy movie", props.Content);
        }

        [Fact]
        public void GetBuyButtonProps_LowBalance_ReturnsDisabled()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, InsufficientBalance);

            Assert.False(props.IsEnabled);
        }

        [Fact]
        public void GetBuyButtonProps_LowBalance_ReturnsBalanceToolTip()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, InsufficientBalance);

            Assert.Contains("balance is too low", props.ToolTip);
        }

        [Fact]
        public void GetBuyButtonProps_LowBalance_ReturnsReducedOpacity()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, InsufficientBalance);

            Assert.Equal(DisabledOpacity, props.Opacity);
        }

        [Fact]
        public void GetBuyButtonProps_ValidPurchase_ReturnsBuyMovieContent()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, SufficientBalance);

            Assert.Equal("Buy movie", props.Content);
        }

        [Fact]
        public void GetBuyButtonProps_ValidPurchase_ReturnsEnabled()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, SufficientBalance);

            Assert.True(props.IsEnabled);
        }

        [Fact]
        public void GetBuyButtonProps_ValidPurchase_ReturnsNullToolTip()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, SufficientBalance);

            Assert.Null(props.ToolTip);
        }

        [Fact]
        public void GetBuyButtonProps_ValidPurchase_ReturnsFullOpacity()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };
            mockMovieRepo.Setup(repository => repository.UserOwnsMovie(ValidUserId, ValidMovieId)).Returns(false);

            var props = service.GetBuyButtonProps(movie, ValidUserId, true, SufficientBalance);

            Assert.Equal(FullOpacity, props.Opacity);
        }

        [Fact]
        public void PurchaseMovie_ValidPurchase_CallsRepository()
        {
            var movie = new Movie { ID = ValidMovieId, Price = MoviePrice, ActiveSaleDiscountPercent = NoDiscount };

            service.PurchaseMovie(ValidUserId, movie);

            mockMovieRepo.Verify(repository => repository.PurchaseMovie(ValidUserId, ValidMovieId, MoviePrice), Times.Once);
        }
    }
}
