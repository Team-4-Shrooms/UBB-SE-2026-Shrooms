using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class ActiveSalesRepoTests
    {
        private readonly ActiveSalesRepo repo;

        public ActiveSalesRepoTests()
        {
            repo = new ActiveSalesRepo();
        }

        [Fact]
        public void GetCurrentSales_ExecutesAndReturnsList()
        {
            // Integration test
            var result = repo.GetCurrentSales();
            Assert.NotNull(result);
        }
    }
}
