using System;
using System.Collections.Generic;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class MovieReviewServiceTests
    {
        private readonly Mock<IReviewRepository> mockReviewRepo;
        private readonly MovieReviewService service;

        public MovieReviewServiceTests()
        {
            mockReviewRepo = new Mock<IReviewRepository>();
            service = new MovieReviewService(mockReviewRepo.Object);
        }

        [Fact]
        public void GetReviewsForMovie_CallsRepository()
        {
            mockReviewRepo.Setup(r => r.GetReviewsForMovie(1)).Returns(new List<MovieReview>());

            var reviews = service.GetReviewsForMovie(1);

            mockReviewRepo.Verify(r => r.GetReviewsForMovie(1), Times.Once);
            Assert.NotNull(reviews);
        }

        [Fact]
        public void AddReview_NotLoggedIn_ThrowsException()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddReview(1, 0, 5, "Good"));
            Assert.Contains("must be logged in", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(-5)]
        public void AddReview_InvalidRating_ThrowsException(int rating)
        {
            var ex = Assert.Throws<InvalidOperationException>(() => service.AddReview(1, 1, rating, "Good"));
            Assert.Contains("must be between 1 and 10", ex.Message);
        }

        [Fact]
        public void AddReview_ValidReview_CallsRepository()
        {
            service.AddReview(1, 1, 8, "Great movie!");

            mockReviewRepo.Verify(r => r.AddReview(1, 1, 8, "Great movie!"), Times.Once);
        }

        [Fact]
        public void GetReviewCount_DelegatesToRepository()
        {
            mockReviewRepo.Setup(r => r.GetReviewCount(42)).Returns(7);

            var count = service.GetReviewCount(42);

            Assert.Equal(7, count);
            mockReviewRepo.Verify(r => r.GetReviewCount(42), Times.Once);
        }

        [Fact]
        public void GetReviewCounts_DelegatesToRepository()
        {
            var ids = new List<int> { 1, 2 };
            var expected = new Dictionary<int, int> { { 1, 3 }, { 2, 5 } };
            mockReviewRepo.Setup(r => r.GetReviewCounts(ids)).Returns(expected);

            var result = service.GetReviewCounts(ids);

            Assert.Equal(expected, result);
            mockReviewRepo.Verify(r => r.GetReviewCounts(ids), Times.Once);
        }

        [Fact]
        public void BuildStarDistributionTooltip_NoReviews_ReturnsNoReviewsMessage()
        {
            mockReviewRepo.Setup(r => r.GetStarRatingBuckets(1)).Returns(new int[11]);

            var tooltip = service.BuildStarDistributionTooltip(1);

            Assert.Equal("No reviews yet.", tooltip);
        }

        [Fact]
        public void BuildStarDistributionTooltip_WithReviews_ReturnsFormattedDistribution()
        {
            var buckets = new int[11];
            buckets[10] = 2;
            buckets[8] = 3;
            buckets[5] = 1;
            mockReviewRepo.Setup(r => r.GetStarRatingBuckets(1)).Returns(buckets);

            var tooltip = service.BuildStarDistributionTooltip(1);

            Assert.StartsWith("Rating distribution:", tooltip);
            Assert.Contains("10: 2", tooltip);
            Assert.Contains("8: 3", tooltip);
            Assert.Contains("5: 1", tooltip);
            Assert.Contains("1: 0", tooltip);
        }
    }
}
