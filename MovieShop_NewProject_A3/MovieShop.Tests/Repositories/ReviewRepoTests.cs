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
    }
}
