using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class InventoryRepoTests
    {
        private readonly InventoryRepo repository;

        public InventoryRepoTests()
        {
            repository = new InventoryRepo();
        }

        [Fact]
        public void GetOwnedMovies_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.GetOwnedMovies(1));
            Assert.Null(exception);
        }

        [Fact]
        public void GetOwnedTickets_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.GetOwnedTickets(1));
            Assert.Null(exception);
        }
    }
}
