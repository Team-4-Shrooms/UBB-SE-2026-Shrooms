using System.Collections.Generic;
using System.Linq;
using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class MarketplaceViewModelTests
    {
        private readonly Mock<IMarketplaceService> mockService;

        public MarketplaceViewModelTests()
        {
            mockService = new Mock<IMarketplaceService>();
            mockService
                .Setup(s => s.FilterByCategory(It.IsAny<IEnumerable<Equipment>>(), It.IsAny<string?>()))
                .Returns((IEnumerable<Equipment> items, string? category) =>
                    string.IsNullOrEmpty(category) || category == "All"
                        ? items.ToList()
                        : items.Where(item => item.Category == category).ToList());
        }

        [Fact]
        public void Constructor_LoadsData()
        {
            var items = new List<Equipment>
            {
                new Equipment { ID = 1, Category = "Camera" },
                new Equipment { ID = 2, Category = "Lighting" }
            };
            mockService.Setup(s => s.GetAvailableEquipment()).Returns(items);

            var viewModel = new MarketplaceViewModel(mockService.Object);

            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void LoadData_EmptyResult_SetsEmptyList()
        {
            mockService.Setup(s => s.GetAvailableEquipment()).Returns(new List<Equipment>());

            var viewModel = new MarketplaceViewModel(mockService.Object);

            Assert.Empty(viewModel.AvailableItems);
        }

        [Fact]
        public void FilterByCategory_NullOrAll_ReturnsAllItems()
        {
            var items = new List<Equipment>
            {
                new Equipment { ID = 1, Category = "Camera" },
                new Equipment { ID = 2, Category = "Lighting" }
            };
            mockService.Setup(s => s.GetAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(mockService.Object);

            viewModel.FilterByCategory("All");
            Assert.Equal(2, viewModel.AvailableItems.Count);

            viewModel.FilterByCategory(null);
            Assert.Equal(2, viewModel.AvailableItems.Count);

            viewModel.FilterByCategory(string.Empty);
            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void FilterByCategory_SpecificCategory_FiltersCorrectly()
        {
            var items = new List<Equipment>
            {
                new Equipment { ID = 1, Category = "Camera" },
                new Equipment { ID = 2, Category = "Lighting" },
                new Equipment { ID = 3, Category = "Lighting" }
            };
            mockService.Setup(s => s.GetAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(mockService.Object);

            viewModel.FilterByCategory("Lighting");

            Assert.Equal(2, viewModel.AvailableItems.Count);
            Assert.All(viewModel.AvailableItems, item => Assert.Equal("Lighting", item.Category));
        }

        [Fact]
        public void StatusMessage_UserNotLoggedIn_ReturnsLogInMessage()
        {
            SessionManager.CurrentUserID = 0;
            mockService.Setup(s => s.GetAvailableEquipment()).Returns(new List<Equipment>());
            var viewModel = new MarketplaceViewModel(mockService.Object);

            var message = viewModel.StatusMessage;

            Assert.Equal("Please log in to purchase equipment.", message);
        }

        [Fact]
        public void StatusMessage_UserLoggedIn_ReturnsEmptyMessage()
        {
            SessionManager.CurrentUserID = 1;
            mockService.Setup(s => s.GetAvailableEquipment()).Returns(new List<Equipment>());
            var viewModel = new MarketplaceViewModel(mockService.Object);

            var message = viewModel.StatusMessage;

            Assert.Empty(message);
        }

        [Fact]
        public void UserBalance_ReturnsSessionBalance()
        {
            SessionManager.CurrentUserBalance = 150m;
            mockService.Setup(s => s.GetAvailableEquipment()).Returns(new List<Equipment>());
            var viewModel = new MarketplaceViewModel(mockService.Object);

            var balance = viewModel.UserBalance;

            Assert.Equal(150m, balance);
        }
    }
}
