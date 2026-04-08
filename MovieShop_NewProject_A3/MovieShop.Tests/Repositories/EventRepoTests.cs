using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class EventRepoTests
    {
        private readonly EventRepo repo;

        public EventRepoTests()
        {
            repo = new EventRepo();
        }

        [Fact]
        public void GetAllEvents_ExecutesAndReturnsList()
        {
            var result = repo.GetAllEvents();
            Assert.NotNull(result);
        }
    }
}
