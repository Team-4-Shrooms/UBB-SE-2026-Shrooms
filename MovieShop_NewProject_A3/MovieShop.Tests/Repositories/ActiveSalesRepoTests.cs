using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class ActiveSalesRepoTests
    {
        private readonly ActiveSalesRepo repository;

        public ActiveSalesRepoTests()
        {
            repository = new ActiveSalesRepo();
        }

        [Fact]
        public void GetCurrentSales_ExecutesAndReturnsList()
        {
            // Integration test
            var result = repository.GetCurrentSales();
            Assert.NotNull(result);
        }
    }
}
