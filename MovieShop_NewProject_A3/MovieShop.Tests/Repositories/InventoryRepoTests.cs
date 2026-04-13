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

        [Fact]
        public void RemoveOwnedMovie_InvalidUserId_DoesNotThrow()
        {
            var exception = Record.Exception(() => repository.RemoveOwnedMovie(0, 1));
            Assert.Null(exception);
        }

        [Fact]
        public void RemoveOwnedTicket_InvalidUserId_DoesNotThrow()
        {
            var exception = Record.Exception(() => repository.RemoveOwnedTicket(-1, 1));
            Assert.Null(exception);
        }
    }
}
