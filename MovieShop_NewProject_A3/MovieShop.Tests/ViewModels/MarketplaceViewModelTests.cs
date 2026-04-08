using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class MarketplaceViewModelTests
    {
        private readonly Mock<IEquipmentRepository> _mockRepo;

        public MarketplaceViewModelTests()
        {
            _mockRepo = new Mock<IEquipmentRepository>();
        }

        [Fact]
        public void Constructor_LoadsData()
        {
            // Arrange
            var items = new List<Equipment>
            {
                new Equipment { ID = 1, Category = "Camera" },
                new Equipment { ID = 2, Category = "Lighting" }
            };
            _mockRepo.Setup(r => r.FetchAvailableEquipment()).Returns(items);

            // Act
            var viewModel = new MarketplaceViewModel(_mockRepo.Object);

            // Assert
            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void LoadData_NullReturn_SetsEmptyList()
        {
            // Arrange
            _mockRepo.Setup(r => r.FetchAvailableEquipment()).Returns((List<Equipment>)null);

            // Act
            var viewModel = new MarketplaceViewModel(_mockRepo.Object);

            // Assert
            Assert.Empty(viewModel.AvailableItems);
        }

        [Fact]
        public void FilterByCategory_NullOrAll_ReturnsAllItems()
        {
            // Arrange
            var items = new List<Equipment>
            {
                new Equipment { ID = 1, Category = "Camera" },
                new Equipment { ID = 2, Category = "Lighting" }
            };
            _mockRepo.Setup(r => r.FetchAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(_mockRepo.Object);

            // Act
            viewModel.FilterByCategory("All");
            Assert.Equal(2, viewModel.AvailableItems.Count);

            viewModel.FilterByCategory(null);
            Assert.Equal(2, viewModel.AvailableItems.Count);
            
            viewModel.FilterByCategory("");
            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void FilterByCategory_SpecificCategory_FiltersCorrectly()
        {
            // Arrange
            var items = new List<Equipment>
            {
                new Equipment { ID = 1, Category = "Camera" },
                new Equipment { ID = 2, Category = "Lighting" },
                new Equipment { ID = 3, Category = "Lighting" }
            };
            _mockRepo.Setup(r => r.FetchAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(_mockRepo.Object);

            // Act
            viewModel.FilterByCategory("Lighting");

            // Assert
            Assert.Equal(2, viewModel.AvailableItems.Count);
            Assert.All(viewModel.AvailableItems, item => Assert.Equal("Lighting", item.Category));
        }

        [Fact]
        public void StatusMessage_UserNotLoggedIn_ReturnsLogInMessage()
        {
            // Arrange
            SessionManager.CurrentUserID = 0;
            var viewModel = new MarketplaceViewModel(_mockRepo.Object);

            // Act
            var message = viewModel.StatusMessage;

            // Assert
            Assert.Equal("Please log in to purchase equipment.", message);
        }

        [Fact]
        public void StatusMessage_UserLoggedIn_ReturnsEmptyMessage()
        {
            // Arrange
            SessionManager.CurrentUserID = 1;
            var viewModel = new MarketplaceViewModel(_mockRepo.Object);

            // Act
            var message = viewModel.StatusMessage;

            // Assert
            Assert.Empty(message);
        }
        
        [Fact]
        public void UserBalance_ReturnsSessionBalance()
        {
            // Arrange
            SessionManager.CurrentUserBalance = 150m;
            var viewModel = new MarketplaceViewModel(_mockRepo.Object);

            // Act
            var balance = viewModel.UserBalance;

            // Assert
            Assert.Equal(150m, balance);
        }
    }
}
