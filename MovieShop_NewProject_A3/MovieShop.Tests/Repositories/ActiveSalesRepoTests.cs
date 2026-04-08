using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class ActiveSalesRepoTests
    {
        private readonly ActiveSalesRepo _repo;

        public ActiveSalesRepoTests()
        {
            _repo = new ActiveSalesRepo();
        }

        [Fact]
        public void GetCurrentSales_ExecutesAndReturnsList()
        {
            // Integration test
            var result = _repo.GetCurrentSales();
            Assert.NotNull(result);
        }
    }
}
