using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class InventoryRepoTests
    {
        private const int TestUserId = 1;

        private readonly InventoryRepo repository;

        public InventoryRepoTests()
        {
            repository = new InventoryRepo();
        }

        [Fact]
        public void GetOwnedMovies_ValidUser_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.GetOwnedMovies(TestUserId));
            Assert.Null(exception);
        }

        [Fact]
        public void GetOwnedTickets_ValidUser_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.GetOwnedTickets(TestUserId));
            Assert.Null(exception);
        }
    }
}
