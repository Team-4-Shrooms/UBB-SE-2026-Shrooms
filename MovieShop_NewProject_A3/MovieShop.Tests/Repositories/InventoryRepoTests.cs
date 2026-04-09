using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class InventoryRepoTests
    {
        private readonly InventoryRepo repo;

        public InventoryRepoTests()
        {
            repo = new InventoryRepo();
        }

        [Fact]
        public void GetOwnedMovies_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repo.GetOwnedMovies(1));
            Assert.Null(exception);
        }

        [Fact]
        public void GetOwnedTickets_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repo.GetOwnedTickets(1));
            Assert.Null(exception);
        }
    }
}
