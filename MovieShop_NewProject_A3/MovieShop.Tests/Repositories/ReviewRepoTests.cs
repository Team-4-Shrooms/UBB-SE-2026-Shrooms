using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class ReviewRepoTests
    {
        private readonly ReviewRepo repo;

        public ReviewRepoTests()
        {
            repo = new ReviewRepo();
        }

        [Fact]
        public void GetReviewsForMovie_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repo.GetReviewsForMovie(1));
            Assert.Null(exception);
        }
    }
}
