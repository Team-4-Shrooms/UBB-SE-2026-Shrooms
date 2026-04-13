using MovieShop.Services;

namespace MovieShop.Tests.Services
{
    public class SaleServiceTests
    {
        [Fact]
        public void CurrentSale_DefaultValue_IsNull()
        {
            var service = new SaleService();

            Assert.Null(service.CurrentSale);
        }

        [Fact]
        public void CurrentSale_SetToNull_ReturnsNull()
        {
            var service = new SaleService { CurrentSale = null };

            Assert.Null(service.CurrentSale);
        }
    }
}
