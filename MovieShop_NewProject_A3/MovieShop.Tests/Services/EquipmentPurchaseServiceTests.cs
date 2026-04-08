using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using System;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class EquipmentPurchaseServiceTests
    {
        private readonly Mock<IEquipmentRepository> _mockRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly EquipmentPurchaseService _service;

        public EquipmentPurchaseServiceTests()
        {
            _mockRepo = new Mock<IEquipmentRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _service = new EquipmentPurchaseService(_mockRepo.Object, _mockUserRepo.Object);
            
            SessionManager.CurrentUserBalance = 0;
        }

        [Fact]
        public void CanAfford_SufficientBalance_ReturnsTrue()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetBalance(1)).Returns(100m);

            // Act
            var result = _service.CanAfford(1, 50m);

            // Assert
            Assert.True(result);
            Assert.Equal(100m, SessionManager.CurrentUserBalance);
        }

        [Fact]
        public void CanAfford_InsufficientBalance_ReturnsFalse()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetBalance(1)).Returns(20m);

            // Act
            var result = _service.CanAfford(1, 50m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void PurchaseEquipment_InsufficientFunds_ThrowsException()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetBalance(1)).Returns(10m);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _service.PurchaseEquipment(1, 1, 50m, "Address"));
            Assert.Contains("Insufficient funds", ex.Message);
        }

        [Fact]
        public void PurchaseEquipment_SufficientFunds_CallsRepositoryAndUpdatesBalance()
        {
            // Arrange
            _mockUserRepo.SetupSequence(r => r.GetBalance(1))
                .Returns(100m) // First call during balance check
                .Returns(50m);  // Call after purchase to update session

            // Act
            _service.PurchaseEquipment(1, 1, 50m, "123 Main St");

            // Assert
            _mockRepo.Verify(r => r.PurchaseEquipment(1, 1, 50m, "123 Main St"), Times.Once);
            Assert.Equal(50m, SessionManager.CurrentUserBalance);
        }
    }
}
