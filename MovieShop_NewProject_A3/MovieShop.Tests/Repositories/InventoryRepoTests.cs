using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class InventoryRepoTests
    {
        private readonly InventoryRepo _repo;

        public InventoryRepoTests()
        {
            _repo = new InventoryRepo();
        }

        [Fact]
        public void GetOwnedMovies_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => _repo.GetOwnedMovies(1));
            Assert.Null(exception);
        }

        [Fact]
        public void GetOwnedTickets_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => _repo.GetOwnedTickets(1));
            Assert.Null(exception);
        }
    }
}
