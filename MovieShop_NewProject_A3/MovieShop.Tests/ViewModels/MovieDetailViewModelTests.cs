using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class MovieDetailViewModelTests
    {
        private const int LoggedInUserId = 1;
        private const int NotLoggedInUserId = 0;
        private const decimal InitialBalance = 100m;
        private const decimal MoviePrice = 10m;
        private const int TestMovieId = 42;
        private const int AlternateMovieId = 7;
        private const int DefaultMovieId = 1;
        private const double FullOpacity = 1.0;
        private const double HalfOpacity = 0.5;
        private const string DefaultBuyButtonText = "Buy movie";
        private const string OwnedButtonText = "Owned";
        private const string OwnedTooltip = "already yours";
        private const string StarTooltip = "\u2605\u2605\u2605";
        private const string InsufficientBalanceTooltip = "Insufficient balance";
        private const string NoMovieError = "No movie selected.";
        private const string NotLoggedInError = "You must be logged in to make a purchase.";
        private const string DbOfflineError = "db offline";

        private readonly Mock<IMoviePurchaseService> purchaseService = new ();
        private readonly Mock<IMovieReviewService> reviewService = new ();
        private readonly Mock<IMovieCatalogService> catalogService = new ();

        public MovieDetailViewModelTests()
        {
            SessionManager.CurrentUserID = LoggedInUserId;
            SessionManager.CurrentUserBalance = InitialBalance;
        }

        [Fact]
        public void Initialize_NullArgs_ClearsMovie()
        {
            var viewModel = CreateViewModel();

            viewModel.Initialize(null);

            Assert.Null(viewModel.Movie);
        }

        [Fact]
        public void Initialize_NullArgs_DisablesBuyButton()
        {
            var viewModel = CreateViewModel();

            viewModel.Initialize(null);

            Assert.False(viewModel.CanBuyMovie);
        }

        [Fact]
        public void Initialize_NullArgs_ResetsButtonText()
        {
            var viewModel = CreateViewModel();

            viewModel.Initialize(null);

            Assert.Equal(DefaultBuyButtonText, viewModel.BuyButtonContent);
        }

        [Fact]
        public void Initialize_ValidArgs_SetsMovie()
        {
            var movie = new Movie { ID = TestMovieId, Title = "Test", Price = MoviePrice };
            SetupValidMovie(movie);

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.Same(movie, viewModel.Movie);
        }

        [Fact]
        public void Initialize_ValidArgs_SetsStarTooltip()
        {
            var movie = new Movie { ID = TestMovieId, Title = "Test", Price = MoviePrice };
            SetupValidMovie(movie);

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.Equal(StarTooltip, viewModel.StarDistributionTooltip);
        }

        [Fact]
        public void Initialize_ValidArgs_EnablesBuyButton()
        {
            var movie = new Movie { ID = TestMovieId, Title = "Test", Price = MoviePrice };
            SetupValidMovie(movie);

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.True(viewModel.CanBuyMovie);
            catalogService.Verify(service => service.ApplyDiscount(movie), Times.Once);
        }

        [Fact]
        public void RefreshBuyButtonState_NoMovie_DisablesBuyButton()
        {
            var viewModel = CreateViewModel();

            viewModel.RefreshBuyButtonState();

            Assert.False(viewModel.CanBuyMovie);
        }

        [Fact]
        public void RefreshBuyButtonState_NoMovie_ResetsButtonText()
        {
            var viewModel = CreateViewModel();

            viewModel.RefreshBuyButtonState();

            Assert.Equal(DefaultBuyButtonText, viewModel.BuyButtonContent);
        }

        [Fact]
        public void RefreshBuyButtonState_MirrorsPurchaseServiceProps_DisablesBuyButton()
        {
            var movie = new Movie { ID = AlternateMovieId, Price = MoviePrice };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(OwnedButtonText, false, OwnedTooltip, HalfOpacity));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.False(viewModel.CanBuyMovie);
        }

        [Fact]
        public void RefreshBuyButtonState_MirrorsPurchaseServiceProps_SetsButtonContent()
        {
            var movie = new Movie { ID = AlternateMovieId, Price = MoviePrice };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(OwnedButtonText, false, OwnedTooltip, HalfOpacity));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.Equal(OwnedButtonText, viewModel.BuyButtonContent);
        }

        [Fact]
        public void RefreshBuyButtonState_MirrorsPurchaseServiceProps_SetsTooltip()
        {
            var movie = new Movie { ID = AlternateMovieId, Price = MoviePrice };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(OwnedButtonText, false, OwnedTooltip, HalfOpacity));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.Equal(OwnedTooltip, viewModel.BuyButtonTooltip);
        }

        [Fact]
        public void RefreshBuyButtonState_MirrorsPurchaseServiceProps_SetsOpacity()
        {
            var movie = new Movie { ID = AlternateMovieId, Price = MoviePrice };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(OwnedButtonText, false, OwnedTooltip, HalfOpacity));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            Assert.Equal(HalfOpacity, viewModel.BuyButtonOpacity);
        }

        [Fact]
        public void TryPurchase_NoMovie_ReturnsFalse()
        {
            var viewModel = CreateViewModel();

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
        }

        [Fact]
        public void TryPurchase_NoMovie_ReturnsNoMovieError()
        {
            var viewModel = CreateViewModel();

            viewModel.TryPurchase(out var error);

            Assert.Equal(NoMovieError, error);
        }

        [Fact]
        public void TryPurchase_NotLoggedIn_ReturnsFalse()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            SetupPurchasableMovie(movie);
            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });
            SessionManager.CurrentUserID = NotLoggedInUserId;

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
        }

        [Fact]
        public void TryPurchase_NotLoggedIn_ReturnsLoginError()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            SetupPurchasableMovie(movie);
            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });
            SessionManager.CurrentUserID = NotLoggedInUserId;

            viewModel.TryPurchase(out var error);

            Assert.Equal(NotLoggedInError, error);
        }

        [Fact]
        public void TryPurchase_CannotBuy_ReturnsFalse()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(DefaultBuyButtonText, false, InsufficientBalanceTooltip, HalfOpacity));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
        }

        [Fact]
        public void TryPurchase_CannotBuy_ReturnsTooltipError()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(DefaultBuyButtonText, false, InsufficientBalanceTooltip, HalfOpacity));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            viewModel.TryPurchase(out var error);

            Assert.Equal(InsufficientBalanceTooltip, error);
            purchaseService.Verify(service => service.PurchaseMovie(It.IsAny<int>(), It.IsAny<Movie>()), Times.Never);
        }

        [Fact]
        public void TryPurchase_Succeeds_ReturnsTrue()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            SetupPurchasableMovie(movie);

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            var result = viewModel.TryPurchase(out var error);

            Assert.True(result);
        }

        [Fact]
        public void TryPurchase_Succeeds_ReturnsEmptyError()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            SetupPurchasableMovie(movie);

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            viewModel.TryPurchase(out var error);

            Assert.Equal(string.Empty, error);
            purchaseService.Verify(service => service.PurchaseMovie(LoggedInUserId, movie), Times.Once);
        }

        [Fact]
        public void TryPurchase_ServiceThrows_ReturnsFalse()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            SetupPurchasableMovie(movie);
            purchaseService
                .Setup(service => service.PurchaseMovie(LoggedInUserId, movie))
                .Throws(new InvalidOperationException(DbOfflineError));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            var result = viewModel.TryPurchase(out var error);

            Assert.False(result);
        }

        [Fact]
        public void TryPurchase_ServiceThrows_ReturnsExceptionMessage()
        {
            var movie = new Movie { ID = DefaultMovieId, Price = MoviePrice };
            SetupPurchasableMovie(movie);
            purchaseService
                .Setup(service => service.PurchaseMovie(LoggedInUserId, movie))
                .Throws(new InvalidOperationException(DbOfflineError));

            var viewModel = CreateViewModel();
            viewModel.Initialize(new MovieDetailNavArgs { Movie = movie });

            viewModel.TryPurchase(out var error);

            Assert.Equal(DbOfflineError, error);
        }

        private MovieDetailViewModel CreateViewModel()
            => new (purchaseService.Object, reviewService.Object, catalogService.Object);

        private void SetupValidMovie(Movie movie)
        {
            catalogService.Setup(service => service.ApplyDiscount(movie));
            reviewService.Setup(service => service.BuildStarDistributionTooltip(TestMovieId)).Returns(StarTooltip);
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(DefaultBuyButtonText, true, null, FullOpacity));
        }

        private void SetupPurchasableMovie(Movie movie)
        {
            purchaseService
                .Setup(service => service.GetBuyButtonProps(movie, LoggedInUserId, true, It.IsAny<decimal>()))
                .Returns(new BuyButtonProps(DefaultBuyButtonText, true, null, FullOpacity));
        }
    }
}
