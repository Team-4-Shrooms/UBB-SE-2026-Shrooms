using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class EquipmentPurchaseServiceTests
    {
        private const int ValidUserId = 1;
        private const int ValidEquipmentId = 1;
        private const decimal SufficientBalance = 100m;
        private const decimal InsufficientBalance = 10m;
        private const decimal LowBalance = 20m;
        private const decimal EquipmentPrice = 50m;
        private const decimal BalanceAfterPurchase = 50m;
        private const string ValidAddress = "123 Main St";

        private readonly Mock<IEquipmentRepository> mockRepo;
        private readonly Mock<IUserRepository> mockUserRepo;
        private readonly EquipmentPurchaseService service;

        public EquipmentPurchaseServiceTests()
        {
            mockRepo = new Mock<IEquipmentRepository>();
            mockUserRepo = new Mock<IUserRepository>();
            service = new EquipmentPurchaseService(mockRepo.Object, mockUserRepo.Object);

            SessionManager.CurrentUserBalance = 0;
        }

        [Fact]
        public void CanAfford_SufficientBalance_ReturnsTrue()
        {
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(SufficientBalance);

            var result = service.CanAfford(ValidUserId, EquipmentPrice);

            Assert.True(result);
        }

        [Fact]
        public void CanAfford_SufficientBalance_UpdatesSessionBalance()
        {
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(SufficientBalance);

            service.CanAfford(ValidUserId, EquipmentPrice);

            Assert.Equal(SufficientBalance, SessionManager.CurrentUserBalance);
        }

        [Fact]
        public void CanAfford_InsufficientBalance_ReturnsFalse()
        {
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(LowBalance);

            var result = service.CanAfford(ValidUserId, EquipmentPrice);

            Assert.False(result);
        }

        [Fact]
        public void PurchaseEquipment_InsufficientFunds_ThrowsException()
        {
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(InsufficientBalance);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                service.PurchaseEquipment(ValidUserId, ValidEquipmentId, EquipmentPrice, ValidAddress));
            Assert.Contains("Insufficient funds", ex.Message);
        }

        [Fact]
        public void PurchaseEquipment_SufficientFunds_CallsRepository()
        {
            mockUserRepo.SetupSequence(repository => repository.GetBalance(ValidUserId))
                .Returns(SufficientBalance)
                .Returns(BalanceAfterPurchase);

            service.PurchaseEquipment(ValidUserId, ValidEquipmentId, EquipmentPrice, ValidAddress);

            mockRepo.Verify(repository => repository.PurchaseEquipment(ValidUserId, ValidEquipmentId, EquipmentPrice, ValidAddress), Times.Once);
        }

        [Fact]
        public void PurchaseEquipment_SufficientFunds_UpdatesSessionBalance()
        {
            mockUserRepo.SetupSequence(repository => repository.GetBalance(ValidUserId))
                .Returns(SufficientBalance)
                .Returns(BalanceAfterPurchase);

            service.PurchaseEquipment(ValidUserId, ValidEquipmentId, EquipmentPrice, ValidAddress);

            Assert.Equal(BalanceAfterPurchase, SessionManager.CurrentUserBalance);
        }
    }
}
