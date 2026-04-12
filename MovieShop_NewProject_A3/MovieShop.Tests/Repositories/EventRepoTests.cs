using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class EventRepoTests
    {
        private readonly EventRepo repository;

        public EventRepoTests()
        {
            repository = new EventRepo();
        }

        [Fact]
        public void GetAllEvents_ValidCall_ReturnsList()
        {
            var result = repository.GetAllEvents();
            Assert.NotNull(result);
        }
    }
}
