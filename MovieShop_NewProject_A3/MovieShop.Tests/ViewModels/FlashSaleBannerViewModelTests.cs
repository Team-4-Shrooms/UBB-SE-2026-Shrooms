using System;
using Moq;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class FlashSaleBannerViewModelTests
    {
        [Fact]
        public void CurrentSale_DelegatesToService()
        {
            var mockService = new Mock<ISaleService>();
            var sale = new FlashSaleViewModel(DateTime.UtcNow.AddMinutes(-1), () => { });
            mockService.Setup(s => s.CurrentSale).Returns(sale);

            var viewModel = new FlashSaleBannerViewModel(mockService.Object);

            Assert.Same(sale, viewModel.CurrentSale);
        }

        [Fact]
        public void CurrentSale_WhenServiceReturnsNull_IsNull()
        {
            var mockService = new Mock<ISaleService>();
            mockService.Setup(s => s.CurrentSale).Returns((FlashSaleViewModel?)null);

            var viewModel = new FlashSaleBannerViewModel(mockService.Object);

            Assert.Null(viewModel.CurrentSale);
        }
    }
}
