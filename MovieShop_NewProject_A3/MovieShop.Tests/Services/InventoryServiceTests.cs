using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IInventoryRepository> _mockRepo;
        private readonly InventoryService _service;

        public InventoryServiceTests()
        {
            _mockRepo = new Mock<IInventoryRepository>();
            _service = new InventoryService(_mockRepo.Object);
        }

        [Fact]
        public void RemoveMovie_InvalidUserId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _service.RemoveMovie(0, 1));
        }

        [Fact]
        public void RemoveMovie_InvalidMovieId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _service.RemoveMovie(1, 0));
        }

        [Fact]
        public void RemoveMovie_ValidInput_CallsRepoAndReturnsRemaining()
        {
            // Arrange
            var movies = new List<Movie> { new Movie { ID = 2 } };
            _mockRepo.Setup(r => r.GetOwnedMovies(1)).Returns(movies);

            // Act
            var result = _service.RemoveMovie(1, 1);

            // Assert
            _mockRepo.Verify(r => r.RemoveOwnedMovie(1, 1), Times.Once);
            Assert.Single(result);
            Assert.Equal(2, result.First().ID);
        }

        [Fact]
        public void RemoveTicket_InvalidUserId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _service.RemoveTicket(0, 1));
        }

        [Fact]
        public void RemoveTicket_InvalidEventId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _service.RemoveTicket(1, 0));
        }

        [Fact]
        public void RemoveTicket_ValidInput_CallsRepoAndReturnsRemaining()
        {
            // Arrange
            var tickets = new List<MovieEvent> { new MovieEvent { ID = 2 } };
            _mockRepo.Setup(r => r.GetOwnedTickets(1)).Returns(tickets);

            // Act
            var result = _service.RemoveTicket(1, 1);

            // Assert
            _mockRepo.Verify(r => r.RemoveOwnedTicket(1, 1), Times.Once);
            Assert.Single(result);
            Assert.Equal(2, result.First().ID);
        }
    }
}
