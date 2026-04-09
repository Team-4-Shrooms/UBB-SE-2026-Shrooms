using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using Xunit;

namespace MovieShop.Tests.Services
{
    public class InventoryServiceTests
    {
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
            Assert.Throws<ArgumentException>(() => service.RemoveMovie(0, 1));
        }

        [Fact]
        public void RemoveMovie_InvalidMovieId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => service.RemoveMovie(1, 0));
        }

        [Fact]
        public void RemoveMovie_ValidInput_CallsRepoAndReturnsRemaining()
        {
            // Arrange
            var movies = new List<Movie> { new Movie { ID = 2 } };
            mockRepo.Setup(r => r.GetOwnedMovies(1)).Returns(movies);

            // Act
            var result = service.RemoveMovie(1, 1);

            // Assert
            mockRepo.Verify(r => r.RemoveOwnedMovie(1, 1), Times.Once);
            Assert.Single(result);
            Assert.Equal(2, result.First().ID);
        }

        [Fact]
        public void RemoveTicket_InvalidUserId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => service.RemoveTicket(0, 1));
        }

        [Fact]
        public void RemoveTicket_InvalidEventId_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => service.RemoveTicket(1, 0));
        }

        [Fact]
        public void RemoveTicket_ValidInput_CallsRepoAndReturnsRemaining()
        {
            // Arrange
            var tickets = new List<MovieEvent> { new MovieEvent { ID = 2 } };
            mockRepo.Setup(r => r.GetOwnedTickets(1)).Returns(tickets);

            // Act
            var result = service.RemoveTicket(1, 1);

            // Assert
            mockRepo.Verify(r => r.RemoveOwnedTicket(1, 1), Times.Once);
            Assert.Single(result);
            Assert.Equal(2, result.First().ID);
        }
    }
}
