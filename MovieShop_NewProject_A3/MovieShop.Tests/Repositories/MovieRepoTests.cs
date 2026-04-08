using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class MovieRepoTests
    {
        private readonly MovieRepo _repo;

        public MovieRepoTests()
        {
            _repo = new MovieRepo();
        }

        [Fact]
        public void GetAllMovies_ExecutesAndReturnsList()
        {
            var result = _repo.GetAllMovies();
            Assert.NotNull(result);
        }
    }
}
