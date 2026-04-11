using System;
using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class MovieDetailViewModelTests
    {
        private readonly Mock<IMoviePurchaseService> purchaseService = new ();
        private readonly Mock<IMovieReviewService> reviewService = new ();
        private readonly Mock<IMovieCatalogService> catalogService = new ();

        public MovieDetailViewModelTests()
        {
            SessionManager.CurrentUserID = 1;
            SessionManager.CurrentUserBalance = 100m;
        }

        [Fact]
        public void Initialize_NullArgs_ClearsMovieAndResetsButton()
        {
            var viewModel = CreateViewModel();

            viewModel.Initialize(null);

            Assert.Null(viewModel.Movie);
            Assert.False(viewModel.CanBuyMovie);
            Assert.Equal("Buy movie", viewModel.BuyButtonContent);
        }

        [Fact]
        public void Initialize_ValidArgs_AppliesDiscountAndLoadsTooltip()
        {
            var movie = new Movie { ID = 42, Title = "Test", Price = 10m };
            catalogService.Setup(service => service.ApplyDiscount(movie));
            reviewService.Setup(service => service.BuildStarDistributionTooltip(42)).Returns("★★★");
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, 1, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps("Buy movie", true, null, 1.0));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.Same(movie, viewModel.Movie);
            Assert.Equal("★★★", viewModel.StarDistributionTooltip);
            Assert.True(viewModel.CanBuyMovie);
            catalogService.Verify(service => service.ApplyDiscount(movie), Times.Once);
        }

        [Fact]
        public void RefreshBuyButtonState_NoMovie_ResetsButton()
        {
            var viewModel = CreateViewModel();

            viewModel.RefreshBuyButtonState();

            Assert.False(viewModel.CanBuyMovie);
            Assert.Equal("Buy movie", viewModel.BuyButtonContent);
        }

        [Fact]
        public void RefreshBuyButtonState_MirrorsPurchaseServiceProps()
        {
            var movie = new Movie { ID = 7, Price = 10m };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, 1, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps("Owned", false, "already yours", 0.5));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.False(viewModel.CanBuyMovie);
            Assert.Equal("Owned", viewModel.BuyButtonContent);
            Assert.Equal("already yours", viewModel.BuyButtonTooltip);
            Assert.Equal(0.5, viewModel.BuyButtonOpacity);
        }

        [Fact]
        public void TryPurchase_NoMovie_ReturnsFalseWithError()
        {
            var viewModel = CreateViewModel();

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
            Assert.Equal("No movie selected.", error);
        }

        [Fact]
        public void TryPurchase_NotLoggedIn_ReturnsFalseWithError()
        {
            SessionManager.CurrentUserID = 0;
            var movie = new Movie { ID = 1, Price = 10m };
            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
            Assert.Equal("You must be logged in to make a purchase.", error);
        }

        [Fact]
        public void TryPurchase_CannotBuy_ReturnsFalseWithTooltipError()
        {
            var movie = new Movie { ID = 1, Price = 10m };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, 1, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps("Buy movie", false, "Insufficient balance", 0.5));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
            Assert.Equal("Insufficient balance", error);
            purchaseService.Verify(service => service.PurchaseMovie(It.IsAny<int>(), It.IsAny<Movie>()), Times.Never);
        }

        [Fact]
        public void TryPurchase_Succeeds_CallsPurchaseService()
        {
            var movie = new Movie { ID = 1, Price = 10m };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, 1, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps("Buy movie", true, null, 1.0));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            var result = viewModel.TryPurchase(out var error);

            Assert.True(result);
            Assert.Equal(string.Empty, error);
            purchaseService.Verify(service => service.PurchaseMovie(1, movie), Times.Once);
        }

        [Fact]
        public void TryPurchase_ServiceThrows_ReturnsFalseWithMessage()
        {
            var movie = new Movie { ID = 1, Price = 10m };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, 1, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps("Buy movie", true, null, 1.0));
            purchaseService
                .Setup(service => service.PurchaseMovie(1, movie))
                .Throws(new InvalidOperationException("db offline"));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
            Assert.Equal("db offline", error);
        }

        private MovieDetailViewModel CreateViewModel()
            => new (purchaseService.Object, reviewService.Object, catalogService.Object);
    }
}
