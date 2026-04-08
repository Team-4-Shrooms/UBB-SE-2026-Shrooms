using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class ReviewRepoTests
    {
        private readonly ReviewRepo _repo;

        public ReviewRepoTests()
        {
            _repo = new ReviewRepo();
        }

        [Fact]
        public void GetReviewsForMovie_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => _repo.GetReviewsForMovie(1));
            Assert.Null(exception);
        }
    }
}
