using System.Collections.Generic;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

namespace MovieShop.Tests.Services
{
    public class ActiveSalesServiceTests
    {
        private readonly Mock<IActiveSalesRepository> salesRepoMock = new Mock<IActiveSalesRepository>();

        [Fact]
        public void GetCurrentSales_ReturnsSalesFromRepository()
        {
            var sales = new List<ActiveSale> { new ActiveSale() };
            salesRepoMock.Setup(salesRepo => salesRepo.GetCurrentSales()).Returns(sales);

            var result = new ActiveSalesService(salesRepoMock.Object).GetCurrentSales();

            Assert.Same(sales, result);
        }

        [Fact]
        public void GetCurrentSales_DelegatesToRepository()
        {
            new ActiveSalesService(salesRepoMock.Object).GetCurrentSales();

            salesRepoMock.Verify(salesRepo => salesRepo.GetCurrentSales(), Times.Once);
        }
    }
}
