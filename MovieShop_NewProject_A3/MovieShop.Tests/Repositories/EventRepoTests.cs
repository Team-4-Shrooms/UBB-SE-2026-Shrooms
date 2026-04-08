using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class EventRepoTests
    {
        private readonly EventRepo _repo;

        public EventRepoTests()
        {
            _repo = new EventRepo();
        }

        [Fact]
        public void GetAllEvents_ExecutesAndReturnsList()
        {
            var result = _repo.GetAllEvents();
            Assert.NotNull(result);
        }
    }
}
