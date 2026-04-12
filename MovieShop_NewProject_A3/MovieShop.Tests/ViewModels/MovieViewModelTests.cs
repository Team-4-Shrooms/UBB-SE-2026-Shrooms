using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class MovieViewModelTests
    {
        private const int NoSaleId = 0;
        private const int ActiveSaleId = 1;
        private const int FutureDayOffset = 1;
        private const decimal BasePrice = 100m;
        private const decimal HighBasePrice = 200m;
        private const int DiscountPercent50 = 50;
        private const int DiscountPercent25 = 25;
        private const int DiscountPercent10 = 10;
        private const decimal ExpectedPriceAfter25Discount = 75m;
        private const decimal ExpectedPriceAfter10Discount = 180m;

        [Fact]
        public void RevertToOriginalPrice_ActiveSale_ResetsSaleId()
        {
            var viewModel = new MovieViewModel();
            viewModel.ApplyDatabaseSale(DiscountPercent50, DateTime.Now.AddDays(FutureDayOffset));

            viewModel.RevertToOriginalPrice();

            Assert.Equal(NoSaleId, viewModel.SaleID);
        }

        [Fact]
        public void RevertToOriginalPrice_ActiveSale_ClearsSaleTimer()
        {
            var viewModel = new MovieViewModel();
            viewModel.ApplyDatabaseSale(DiscountPercent50, DateTime.Now.AddDays(FutureDayOffset));

            viewModel.RevertToOriginalPrice();

            Assert.Null(viewModel.SaleTimer);
        }

        [Fact]
        public void ApplyDatabaseSale_ValidSale_SetsDiscountPercent()
        {
            var viewModel = new MovieViewModel();

            viewModel.ApplyDatabaseSale(DiscountPercent50, DateTime.Now.AddDays(FutureDayOffset));

            Assert.Equal(DiscountPercent50, viewModel.DiscountPercent);
        }

        [Fact]
        public void ApplyDatabaseSale_ValidSale_SetsSaleId()
        {
            var viewModel = new MovieViewModel();

            viewModel.ApplyDatabaseSale(DiscountPercent50, DateTime.Now.AddDays(FutureDayOffset));

            Assert.Equal(ActiveSaleId, viewModel.SaleID);
        }

        [Fact]
        public void ApplyDatabaseSale_ValidSale_SetsSaleTimer()
        {
            var viewModel = new MovieViewModel();

            viewModel.ApplyDatabaseSale(DiscountPercent50, DateTime.Now.AddDays(FutureDayOffset));

            Assert.NotNull(viewModel.SaleTimer);
        }

        [Fact]
        public void DisplayPrice_WithActiveSale_ReturnsSalePrice()
        {
            var viewModel = new MovieViewModel { BasePrice = BasePrice };
            viewModel.ApplyDatabaseSale(DiscountPercent25, DateTime.Now.AddDays(FutureDayOffset));

            var displayPrice = viewModel.DisplayPrice;

            Assert.Equal(ExpectedPriceAfter25Discount, displayPrice);
        }

        [Fact]
        public void DisplayPrice_NoActiveSale_ReturnsBasePrice()
        {
            var viewModel = new MovieViewModel { BasePrice = BasePrice };

            var displayPrice = viewModel.DisplayPrice;

            Assert.Equal(BasePrice, displayPrice);
        }

        [Fact]
        public void SalePrice_ValidDiscount_CalculatesCorrectly()
        {
            var viewModel = new MovieViewModel { BasePrice = HighBasePrice, DiscountPercent = DiscountPercent10 };

            var salePrice = viewModel.SalePrice;

            Assert.Equal(ExpectedPriceAfter10Discount, salePrice);
        }

        [Fact]
        public void IsSaleActive_NoSale_ReturnsFalse()
        {
            var viewModel = new MovieViewModel { SaleID = NoSaleId };

            Assert.False(viewModel.IsSaleActive);
        }

        [Fact]
        public void ShowStrike_NoSale_ReturnsFalse()
        {
            var viewModel = new MovieViewModel { SaleID = NoSaleId };

            Assert.False(viewModel.ShowStrike);
        }

        [Fact]
        public void SaleBadgeText_NoSale_ReturnsEmpty()
        {
            var viewModel = new MovieViewModel { SaleID = NoSaleId };

            Assert.Equal(string.Empty, viewModel.SaleBadgeText);
        }

        [Fact]
        public void IsSaleActive_WithActiveSale_ReturnsTrue()
        {
            var viewModel = new MovieViewModel { SaleID = ActiveSaleId, DiscountPercent = DiscountPercent25 };

            Assert.True(viewModel.IsSaleActive);
        }

        [Fact]
        public void ShowStrike_WithActiveSale_ReturnsTrue()
        {
            var viewModel = new MovieViewModel { SaleID = ActiveSaleId, DiscountPercent = DiscountPercent25 };

            Assert.True(viewModel.ShowStrike);
        }

        [Fact]
        public void SaleBadgeText_WithActiveSale_ReturnsFormattedString()
        {
            var viewModel = new MovieViewModel { SaleID = ActiveSaleId, DiscountPercent = DiscountPercent25 };

            Assert.Equal("25% OFF", viewModel.SaleBadgeText);
        }
    }
}
