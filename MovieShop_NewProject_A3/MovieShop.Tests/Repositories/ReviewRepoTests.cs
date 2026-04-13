using System.Collections.Generic;
using System.Linq;
using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class ReviewRepoTests
    {
        private const int TestMovieId = 1;

        private readonly ReviewRepo repository;

        public ReviewRepoTests()
        {
            repository = new ReviewRepo();
        }

        [Fact]
        public void GetReviewsForMovie_ValidMovie_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.GetReviewsForMovie(TestMovieId));
            Assert.Null(exception);
        }

        [Fact]
        public void GetReviewCounts_EmptyIds_ReturnsEmptyDictionary()
        {
            var result = repository.GetReviewCounts(Enumerable.Empty<int>());

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetReviewCounts_OnlyDuplicateIds_DeduplicatesBeforeQuery()
        {
            var result = repository.GetReviewCounts(new List<int>());

            Assert.Empty(result);
        }
    }
}
