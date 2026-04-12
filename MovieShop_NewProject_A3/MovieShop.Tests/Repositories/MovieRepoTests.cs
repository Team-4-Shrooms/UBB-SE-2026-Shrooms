using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class MovieRepoTests
    {
        private readonly MovieRepo repository;

        public MovieRepoTests()
        {
            repository = new MovieRepo();
        }

        [Fact]
        public void GetAllMovies_ValidCall_ReturnsList()
        {
            var result = repository.GetAllMovies();
            Assert.NotNull(result);
        }
    }
}
