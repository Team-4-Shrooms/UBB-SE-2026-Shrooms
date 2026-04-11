using MovieShop.Repositories;
using Xunit;

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
        public void GetAllMovies_ExecutesAndReturnsList()
        {
            var result = repository.GetAllMovies();
            Assert.NotNull(result);
        }
    }
}
