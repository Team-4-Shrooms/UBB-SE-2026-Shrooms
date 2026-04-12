using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class MovieReviewServiceTests
    {
        private const int ValidMovieId = 1;
        private const int ValidUserId = 1;
        private const int NotLoggedInUserId = 0;
        private const int ValidRating = 8;
        private const int InvalidRatingLow = 0;
        private const int InvalidRatingHigh = 11;
        private const int InvalidRatingNegative = -5;
        private const int ReviewCountMovieId = 42;
        private const int ExpectedReviewCount = 7;
        private const int MovieIdOne = 1;
        private const int MovieIdTwo = 2;
        private const int ReviewCountOne = 3;
        private const int ReviewCountTwo = 5;
        private const int BucketSize = 11;
        private const int TenStarIndex = 10;
        private const int TenStarCount = 2;
        private const int EightStarIndex = 8;
        private const int EightStarCount = 3;
        private const int FiveStarIndex = 5;
        private const int FiveStarCount = 1;
        private const int DefaultRating = 5;

        private static readonly string ValidReviewText = "Great movie!";

        private readonly Mock<IReviewRepository> mockReviewRepo;
        private readonly MovieReviewService service;

        public MovieReviewServiceTests()
        {
            mockReviewRepo = new Mock<IReviewRepository>();
            service = new MovieReviewService(mockReviewRepo.Object);
        }

        [Fact]
        public void GetReviewsForMovie_ValidMovieId_ReturnsNonNull()
        {
            mockReviewRepo.Setup(repository => repository.GetReviewsForMovie(ValidMovieId)).Returns(new List<MovieReview>());

            var reviews = service.GetReviewsForMovie(ValidMovieId);

            Assert.NotNull(reviews);
        }

        [Fact]
        public void GetReviewsForMovie_ValidMovieId_CallsRepository()
        {
            mockReviewRepo.Setup(repository => repository.GetReviewsForMovie(ValidMovieId)).Returns(new List<MovieReview>());

            service.GetReviewsForMovie(ValidMovieId);

            mockReviewRepo.Verify(repository => repository.GetReviewsForMovie(ValidMovieId), Times.Once);
        }

        [Fact]
        public void AddReview_NotLoggedIn_ThrowsException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                service.AddReview(ValidMovieId, NotLoggedInUserId, DefaultRating, "Good"));
            Assert.Contains("must be logged in", ex.Message);
        }

        [Theory]
        [InlineData(InvalidRatingLow)]
        [InlineData(InvalidRatingHigh)]
        [InlineData(InvalidRatingNegative)]
        public void AddReview_InvalidRating_ThrowsException(int rating)
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                service.AddReview(ValidMovieId, ValidUserId, rating, "Good"));
            Assert.Contains("must be between 1 and 10", ex.Message);
        }

        [Fact]
        public void AddReview_ValidReview_CallsRepository()
        {
            service.AddReview(ValidMovieId, ValidUserId, ValidRating, ValidReviewText);

            mockReviewRepo.Verify(repository => repository.AddReview(ValidMovieId, ValidUserId, ValidRating, ValidReviewText), Times.Once);
        }

        [Fact]
        public void GetReviewCount_ValidMovieId_ReturnsExpectedCount()
        {
            mockReviewRepo.Setup(repository => repository.GetReviewCount(ReviewCountMovieId)).Returns(ExpectedReviewCount);

            var count = service.GetReviewCount(ReviewCountMovieId);

            Assert.Equal(ExpectedReviewCount, count);
        }

        [Fact]
        public void GetReviewCount_ValidMovieId_CallsRepository()
        {
            mockReviewRepo.Setup(repository => repository.GetReviewCount(ReviewCountMovieId)).Returns(ExpectedReviewCount);

            service.GetReviewCount(ReviewCountMovieId);

            mockReviewRepo.Verify(repository => repository.GetReviewCount(ReviewCountMovieId), Times.Once);
        }

        [Fact]
        public void GetReviewCounts_ValidIds_ReturnsExpectedCounts()
        {
            var ids = new List<int> { MovieIdOne, MovieIdTwo };
            var expected = new Dictionary<int, int> { { MovieIdOne, ReviewCountOne }, { MovieIdTwo, ReviewCountTwo } };
            mockReviewRepo.Setup(repository => repository.GetReviewCounts(ids)).Returns(expected);

            var result = service.GetReviewCounts(ids);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetReviewCounts_ValidIds_CallsRepository()
        {
            var ids = new List<int> { MovieIdOne, MovieIdTwo };
            var expected = new Dictionary<int, int> { { MovieIdOne, ReviewCountOne }, { MovieIdTwo, ReviewCountTwo } };
            mockReviewRepo.Setup(repository => repository.GetReviewCounts(ids)).Returns(expected);

            service.GetReviewCounts(ids);

            mockReviewRepo.Verify(repository => repository.GetReviewCounts(ids), Times.Once);
        }

        [Fact]
        public void BuildStarDistributionTooltip_NoReviews_ReturnsNoReviewsMessage()
        {
            mockReviewRepo.Setup(repository => repository.GetStarRatingBuckets(ValidMovieId)).Returns(new int[BucketSize]);

            var tooltip = service.BuildStarDistributionTooltip(ValidMovieId);

            Assert.Equal("No reviews yet.", tooltip);
        }

        [Fact]
        public void BuildStarDistributionTooltip_WithReviews_StartsWithHeader()
        {
            var buckets = new int[BucketSize];
            buckets[TenStarIndex] = TenStarCount;
            buckets[EightStarIndex] = EightStarCount;
            buckets[FiveStarIndex] = FiveStarCount;
            mockReviewRepo.Setup(repository => repository.GetStarRatingBuckets(ValidMovieId)).Returns(buckets);

            var tooltip = service.BuildStarDistributionTooltip(ValidMovieId);

            Assert.StartsWith("Rating distribution:", tooltip);
        }

        [Fact]
        public void BuildStarDistributionTooltip_WithReviews_ContainsTenStarCount()
        {
            var buckets = new int[BucketSize];
            buckets[TenStarIndex] = TenStarCount;
            buckets[EightStarIndex] = EightStarCount;
            buckets[FiveStarIndex] = FiveStarCount;
            mockReviewRepo.Setup(repository => repository.GetStarRatingBuckets(ValidMovieId)).Returns(buckets);

            var tooltip = service.BuildStarDistributionTooltip(ValidMovieId);

            Assert.Contains("10: 2", tooltip);
        }

        [Fact]
        public void BuildStarDistributionTooltip_WithReviews_ContainsEightStarCount()
        {
            var buckets = new int[BucketSize];
            buckets[TenStarIndex] = TenStarCount;
            buckets[EightStarIndex] = EightStarCount;
            buckets[FiveStarIndex] = FiveStarCount;
            mockReviewRepo.Setup(repository => repository.GetStarRatingBuckets(ValidMovieId)).Returns(buckets);

            var tooltip = service.BuildStarDistributionTooltip(ValidMovieId);

            Assert.Contains("8: 3", tooltip);
        }

        [Fact]
        public void BuildStarDistributionTooltip_WithReviews_ContainsFiveStarCount()
        {
            var buckets = new int[BucketSize];
            buckets[TenStarIndex] = TenStarCount;
            buckets[EightStarIndex] = EightStarCount;
            buckets[FiveStarIndex] = FiveStarCount;
            mockReviewRepo.Setup(repository => repository.GetStarRatingBuckets(ValidMovieId)).Returns(buckets);

            var tooltip = service.BuildStarDistributionTooltip(ValidMovieId);

            Assert.Contains("5: 1", tooltip);
        }

        [Fact]
        public void BuildStarDistributionTooltip_WithReviews_ContainsZeroCountEntries()
        {
            var buckets = new int[BucketSize];
            buckets[TenStarIndex] = TenStarCount;
            buckets[EightStarIndex] = EightStarCount;
            buckets[FiveStarIndex] = FiveStarCount;
            mockReviewRepo.Setup(repository => repository.GetStarRatingBuckets(ValidMovieId)).Returns(buckets);

            var tooltip = service.BuildStarDistributionTooltip(ValidMovieId);

            Assert.Contains("1: 0", tooltip);
        }
    }
}
