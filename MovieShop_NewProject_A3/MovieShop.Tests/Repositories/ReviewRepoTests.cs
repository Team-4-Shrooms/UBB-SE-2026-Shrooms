using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class ReviewRepoTests
    {
        private readonly ReviewRepo repository;

        public ReviewRepoTests()
        {
            repository = new ReviewRepo();
        }

        [Fact]
        public void GetReviewsForMovie_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.GetReviewsForMovie(1));
            Assert.Null(exception);
        }
    }
}
