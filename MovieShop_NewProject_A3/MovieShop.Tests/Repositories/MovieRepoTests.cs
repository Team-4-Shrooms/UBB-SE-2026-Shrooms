using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class MovieRepoTests
    {
        private readonly MovieRepo repo;

        public MovieRepoTests()
        {
            repo = new MovieRepo();
        }

        [Fact]
        public void GetAllMovies_ExecutesAndReturnsList()
        {
            var result = repo.GetAllMovies();
            Assert.NotNull(result);
        }
    }
}
