using System;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class EquipmentPurchaseServiceTests
    {
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
            // Arrange
            mockUserRepo.Setup(r => r.GetBalance(1)).Returns(100m);

            // Act
            var result = service.CanAfford(1, 50m);

            // Assert
            Assert.True(result);
            Assert.Equal(100m, SessionManager.CurrentUserBalance);
        }

        [Fact]
        public void CanAfford_InsufficientBalance_ReturnsFalse()
        {
            // Arrange
            mockUserRepo.Setup(r => r.GetBalance(1)).Returns(20m);

            // Act
            var result = service.CanAfford(1, 50m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void PurchaseEquipment_InsufficientFunds_ThrowsException()
        {
            // Arrange
            mockUserRepo.Setup(r => r.GetBalance(1)).Returns(10m);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => service.PurchaseEquipment(1, 1, 50m, "Address"));
            Assert.Contains("Insufficient funds", ex.Message);
        }

        [Fact]
        public void PurchaseEquipment_SufficientFunds_CallsRepositoryAndUpdatesBalance()
        {
            // Arrange
            mockUserRepo.SetupSequence(r => r.GetBalance(1))
                .Returns(100m) // First call during balance check
                .Returns(50m);  // Call after purchase to update session

            // Act
            service.PurchaseEquipment(1, 1, 50m, "123 Main St");

            // Assert
            mockRepo.Verify(r => r.PurchaseEquipment(1, 1, 50m, "123 Main St"), Times.Once);
            Assert.Equal(50m, SessionManager.CurrentUserBalance);
        }
    }
}
