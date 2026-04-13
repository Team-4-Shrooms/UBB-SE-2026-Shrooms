using System.Collections.Generic;
using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class InventoryViewModelTests
    {
        private const int LoggedInUserId = 1;
        private const int MovieId = 10;
        private const int EventId = 20;

        private readonly Mock<IInventoryService> mockService;

        public InventoryViewModelTests()
        {
            mockService = new Mock<IInventoryService>();
            SessionManager.CurrentUserID = LoggedInUserId;
        }

        [Fact]
        public void Load_PopulatesCollectionsFromService()
        {
            mockService.Setup(s => s.GetOwnedMovies(LoggedInUserId))
                .Returns(new List<Movie> { new Movie { ID = MovieId } });
            mockService.Setup(s => s.GetOwnedTickets(LoggedInUserId))
                .Returns(new List<MovieEvent> { new MovieEvent { ID = EventId } });
            mockService.Setup(s => s.GetOwnedEquipment(LoggedInUserId))
                .Returns(new List<Equipment> { new Equipment { ID = 30 } });

            var viewModel = new InventoryViewModel(mockService.Object);
            viewModel.Load();

            Assert.Single(viewModel.OwnedMovies);
            Assert.Single(viewModel.OwnedTickets);
            Assert.Single(viewModel.OwnedEquipment);
        }

        [Fact]
        public void Load_WhenNotLoggedIn_ClearsCollections()
        {
            SessionManager.CurrentUserID = 0;
            var viewModel = new InventoryViewModel(mockService.Object);

            viewModel.OwnedMovies.Add(new Movie { ID = 1 });
            viewModel.Load();

            Assert.Empty(viewModel.OwnedMovies);
            mockService.Verify(s => s.GetOwnedMovies(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void RemoveMovie_NullMovie_DoesNothing()
        {
            var viewModel = new InventoryViewModel(mockService.Object);

            viewModel.RemoveMovie(null!);

            mockService.Verify(s => s.RemoveMovie(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void RemoveMovie_WithMovie_UpdatesCollection()
        {
            mockService.Setup(s => s.RemoveMovie(LoggedInUserId, MovieId))
                .Returns(new List<Movie>());
            var viewModel = new InventoryViewModel(mockService.Object);
            viewModel.OwnedMovies.Add(new Movie { ID = MovieId });

            viewModel.RemoveMovie(new Movie { ID = MovieId });

            Assert.Empty(viewModel.OwnedMovies);
        }

        [Fact]
        public void RemoveTicket_NullTicket_DoesNothing()
        {
            var viewModel = new InventoryViewModel(mockService.Object);

            viewModel.RemoveTicket(null!);

            mockService.Verify(s => s.RemoveTicket(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void RemoveTicket_WithTicket_UpdatesCollection()
        {
            mockService.Setup(s => s.RemoveTicket(LoggedInUserId, EventId))
                .Returns(new List<MovieEvent>());
            var viewModel = new InventoryViewModel(mockService.Object);
            viewModel.OwnedTickets.Add(new MovieEvent { ID = EventId });

            viewModel.RemoveTicket(new MovieEvent { ID = EventId });

            Assert.Empty(viewModel.OwnedTickets);
        }
    }
}
