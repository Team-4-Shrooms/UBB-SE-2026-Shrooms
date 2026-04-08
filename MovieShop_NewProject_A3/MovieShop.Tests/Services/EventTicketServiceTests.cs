using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class EventTicketServiceTests
    {
        private readonly Mock<IEventRepository> _mockEventRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly EventTicketService _service;

        public EventTicketServiceTests()
        {
            _mockEventRepo = new Mock<IEventRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _service = new EventTicketService(_mockEventRepo.Object, _mockUserRepo.Object);
        }

        [Fact]
        public void CanBuyTicket_InvalidUserId_ReturnsFalse()
        {
            // Act
            var result = _service.CanBuyTicket(0, new MovieEvent { TicketPrice = 10 });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanBuyTicket_NullEvent_ReturnsFalse()
        {
            // Act
            var result = _service.CanBuyTicket(1, null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanBuyTicket_SufficientBalance_ReturnsTrue()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetBalance(1)).Returns(50m);

            // Act
            var result = _service.CanBuyTicket(1, new MovieEvent { TicketPrice = 20m });

            // Assert
            Assert.True(result);
            Assert.Equal(50m, SessionManager.CurrentUserBalance);
        }

        [Fact]
        public void CanBuyTicket_InsufficientBalance_ReturnsFalse()
        {
            // Arrange
            _mockUserRepo.Setup(r => r.GetBalance(1)).Returns(10m);

            // Act
            var result = _service.CanBuyTicket(1, new MovieEvent { TicketPrice = 20m });

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void PurchaseTicket_ValidPurchase_CallsRepositoryAndUpdatesSession()
        {
            // Arrange
            var movieEvent = new MovieEvent { ID = 1, TicketPrice = 20m };
            _mockUserRepo.Setup(r => r.GetBalance(1)).Returns(30m);

            // Act
            _service.PurchaseTicket(1, movieEvent);

            // Assert
            _mockEventRepo.Verify(r => r.PurchaseTicket(1, 1), Times.Once);
            Assert.Equal(30m, SessionManager.CurrentUserBalance);
        }
    }
}
