using System.Collections.Generic;
using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class MovieReviewsViewModelTests
    {
        private const int MovieId = 42;

        private readonly Mock<IMovieReviewService> mockService;

        public MovieReviewsViewModelTests()
        {
            mockService = new Mock<IMovieReviewService>();
            mockService.Setup(s => s.GetReviewsForMovie(It.IsAny<int>()))
                .Returns(new List<MovieReview>());
            SessionManager.CurrentUserID = 1;
        }

        [Fact]
        public void Initialize_NullMovie_KeepsDefaultTitleAndEmptyReviews()
        {
            var viewModel = new MovieReviewsViewModel(mockService.Object);

            viewModel.Initialize(null);

            Assert.Equal("Reviews", viewModel.Title);
            Assert.False(viewModel.CanAddReview);
            Assert.Empty(viewModel.Reviews);
        }

        [Fact]
        public void Initialize_WithMovie_LoadsReviewsAndSetsTitle()
        {
            mockService.Setup(s => s.GetReviewsForMovie(MovieId))
                .Returns(new List<MovieReview> { new MovieReview { ID = 1 } });
            var viewModel = new MovieReviewsViewModel(mockService.Object);

            viewModel.Initialize(new Movie { ID = MovieId, Title = "Matrix" });

            Assert.Equal("Reviews - Matrix", viewModel.Title);
            Assert.True(viewModel.CanAddReview);
            Assert.Single(viewModel.Reviews);
        }

        [Fact]
        public void TryAddReview_NoMovie_ReturnsError()
        {
            var viewModel = new MovieReviewsViewModel(mockService.Object);

            var result = viewModel.TryAddReview("5", "ok", out var error);

            Assert.False(result);
            Assert.Equal("No movie selected.", error);
        }

        [Fact]
        public void TryAddReview_InvalidRatingText_ReturnsError()
        {
            var viewModel = new MovieReviewsViewModel(mockService.Object);
            viewModel.Initialize(new Movie { ID = MovieId });

            var result = viewModel.TryAddReview("abc", "ok", out var error);

            Assert.False(result);
            Assert.Contains("between", error);
        }

        [Fact]
        public void TryAddReview_OutOfRange_ReturnsError()
        {
            var viewModel = new MovieReviewsViewModel(mockService.Object);
            viewModel.Initialize(new Movie { ID = MovieId });

            var result = viewModel.TryAddReview("11", "ok", out var error);

            Assert.False(result);
            Assert.Contains("must be between", error);
        }

        [Fact]
        public void TryAddReview_Valid_AddsReviewAndReloads()
        {
            mockService.Setup(s => s.GetReviewsForMovie(MovieId))
                .Returns(new List<MovieReview>());
            var viewModel = new MovieReviewsViewModel(mockService.Object);
            viewModel.Initialize(new Movie { ID = MovieId });

            var result = viewModel.TryAddReview("7", "great", out var error);

            Assert.True(result);
            Assert.Equal(string.Empty, error);
            mockService.Verify(s => s.AddReview(MovieId, It.IsAny<int>(), 7, "great"), Times.Once);
        }
    }
}
