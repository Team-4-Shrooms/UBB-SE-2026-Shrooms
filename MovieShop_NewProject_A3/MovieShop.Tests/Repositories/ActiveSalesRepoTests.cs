using MovieShop.Repositories;

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
        public void GetCurrentSales_ValidCall_ReturnsList()
        {
            var result = repository.GetCurrentSales();
            Assert.NotNull(result);
        }
    }
}
