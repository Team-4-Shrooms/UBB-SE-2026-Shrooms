using MovieShop.Repositories;
using Xunit;

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
        public void GetAllEvents_ExecutesAndReturnsList()
        {
            var result = repository.GetAllEvents();
            Assert.NotNull(result);
        }
    }
}
