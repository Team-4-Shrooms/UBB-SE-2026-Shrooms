using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class InventoryServiceTests
    {
        private const int ValidUserId = 1;
        private const int InvalidUserId = 0;
        private const int ValidMovieId = 1;
        private const int InvalidMovieId = 0;
        private const int ValidEventId = 1;
        private const int InvalidEventId = 0;
        private const int RemainingItemId = 2;

        private readonly Mock<IInventoryRepository> mockRepo;
        private readonly InventoryService service;

        public InventoryServiceTests()
        {
            mockRepo = new Mock<IInventoryRepository>();
            service = new InventoryService(mockRepo.Object);
        }

        [Fact]
        public void RemoveMovie_InvalidUserId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => service.RemoveMovie(InvalidUserId, ValidMovieId));
        }

        [Fact]
        public void RemoveMovie_InvalidMovieId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => service.RemoveMovie(ValidUserId, InvalidMovieId));
        }

        [Fact]
        public void RemoveMovie_ValidInput_CallsRepository()
        {
            var movies = new List<Movie> { new Movie { ID = RemainingItemId } };
            mockRepo.Setup(repository => repository.GetOwnedMovies(ValidUserId)).Returns(movies);

            service.RemoveMovie(ValidUserId, ValidMovieId);

            mockRepo.Verify(repository => repository.RemoveOwnedMovie(ValidUserId, ValidMovieId), Times.Once);
        }

        [Fact]
        public void RemoveMovie_ValidInput_ReturnsRemainingMovies()
        {
            var movies = new List<Movie> { new Movie { ID = RemainingItemId } };
            mockRepo.Setup(repository => repository.GetOwnedMovies(ValidUserId)).Returns(movies);

            var result = service.RemoveMovie(ValidUserId, ValidMovieId);

            Assert.Single(result);
        }

        [Fact]
        public void RemoveMovie_ValidInput_ReturnsCorrectRemainingMovie()
        {
            var movies = new List<Movie> { new Movie { ID = RemainingItemId } };
            mockRepo.Setup(repository => repository.GetOwnedMovies(ValidUserId)).Returns(movies);

            var result = service.RemoveMovie(ValidUserId, ValidMovieId);

            Assert.Equal(RemainingItemId, result.First().ID);
        }

        [Fact]
        public void RemoveTicket_InvalidUserId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => service.RemoveTicket(InvalidUserId, ValidEventId));
        }

        [Fact]
        public void RemoveTicket_InvalidEventId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => service.RemoveTicket(ValidUserId, InvalidEventId));
        }

        [Fact]
        public void RemoveTicket_ValidInput_CallsRepository()
        {
            var tickets = new List<MovieEvent> { new MovieEvent { ID = RemainingItemId } };
            mockRepo.Setup(repository => repository.GetOwnedTickets(ValidUserId)).Returns(tickets);

            service.RemoveTicket(ValidUserId, ValidEventId);

            mockRepo.Verify(repository => repository.RemoveOwnedTicket(ValidUserId, ValidEventId), Times.Once);
        }

        [Fact]
        public void RemoveTicket_ValidInput_ReturnsRemainingTickets()
        {
            var tickets = new List<MovieEvent> { new MovieEvent { ID = RemainingItemId } };
            mockRepo.Setup(repository => repository.GetOwnedTickets(ValidUserId)).Returns(tickets);

            var result = service.RemoveTicket(ValidUserId, ValidEventId);

            Assert.Single(result);
        }

        [Fact]
        public void RemoveTicket_ValidInput_ReturnsCorrectRemainingTicket()
        {
            var tickets = new List<MovieEvent> { new MovieEvent { ID = RemainingItemId } };
            mockRepo.Setup(repository => repository.GetOwnedTickets(ValidUserId)).Returns(tickets);

            var result = service.RemoveTicket(ValidUserId, ValidEventId);

            Assert.Equal(RemainingItemId, result.First().ID);
        }
    }
}
