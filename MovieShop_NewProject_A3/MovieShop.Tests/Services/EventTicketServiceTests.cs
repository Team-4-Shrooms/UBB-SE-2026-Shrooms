using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class EventTicketServiceTests
    {
        private const int ValidUserId = 1;
        private const int InvalidUserId = 0;
        private const int ValidEventId = 1;
        private const decimal SufficientBalance = 50m;
        private const decimal InsufficientBalance = 10m;
        private const decimal TicketPrice = 20m;
        private const decimal PostPurchaseBalance = 30m;

        private readonly Mock<IEventRepository> mockEventRepo;
        private readonly Mock<IUserRepository> mockUserRepo;
        private readonly EventTicketService service;

        public EventTicketServiceTests()
        {
            mockEventRepo = new Mock<IEventRepository>();
            mockUserRepo = new Mock<IUserRepository>();
            service = new EventTicketService(mockEventRepo.Object, mockUserRepo.Object);
        }

        [Fact]
        public void CanBuyTicket_InvalidUserId_ReturnsFalse()
        {
            var result = service.CanBuyTicket(InvalidUserId, new MovieEvent { TicketPrice = TicketPrice });

            Assert.False(result);
        }

        [Fact]
        public void CanBuyTicket_NullEvent_ReturnsFalse()
        {
            var result = service.CanBuyTicket(ValidUserId, null);

            Assert.False(result);
        }

        [Fact]
        public void CanBuyTicket_SufficientBalance_ReturnsTrue()
        {
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(SufficientBalance);

            var result = service.CanBuyTicket(ValidUserId, new MovieEvent { TicketPrice = TicketPrice });

            Assert.True(result);
        }

        [Fact]
        public void CanBuyTicket_SufficientBalance_UpdatesSessionBalance()
        {
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(SufficientBalance);

            service.CanBuyTicket(ValidUserId, new MovieEvent { TicketPrice = TicketPrice });

            Assert.Equal(SufficientBalance, SessionManager.CurrentUserBalance);
        }

        [Fact]
        public void CanBuyTicket_InsufficientBalance_ReturnsFalse()
        {
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(InsufficientBalance);

            var result = service.CanBuyTicket(ValidUserId, new MovieEvent { TicketPrice = TicketPrice });

            Assert.False(result);
        }

        [Fact]
        public void PurchaseTicket_ValidPurchase_CallsRepository()
        {
            var movieEvent = new MovieEvent { ID = ValidEventId, TicketPrice = TicketPrice };
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(PostPurchaseBalance);

            service.PurchaseTicket(ValidUserId, movieEvent);

            mockEventRepo.Verify(repository => repository.PurchaseTicket(ValidUserId, ValidEventId), Times.Once);
        }

        [Fact]
        public void PurchaseTicket_ValidPurchase_UpdatesSessionBalance()
        {
            var movieEvent = new MovieEvent { ID = ValidEventId, TicketPrice = TicketPrice };
            mockUserRepo.Setup(repository => repository.GetBalance(ValidUserId)).Returns(PostPurchaseBalance);

            service.PurchaseTicket(ValidUserId, movieEvent);

            Assert.Equal(PostPurchaseBalance, SessionManager.CurrentUserBalance);
        }
    }
}
