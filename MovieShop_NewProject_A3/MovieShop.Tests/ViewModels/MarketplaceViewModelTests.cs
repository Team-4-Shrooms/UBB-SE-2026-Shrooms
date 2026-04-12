using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class MarketplaceViewModelTests
    {
        private const int NotLoggedInUserId = 0;
        private const int LoggedInUserId = 1;
        private const int CameraItemId = 1;
        private const int LightingItemId = 2;
        private const int SecondLightingItemId = 3;
        private const decimal UserBalance = 150m;

        private readonly Mock<IMarketplaceService> mockService;

        public MarketplaceViewModelTests()
        {
            mockService = new Mock<IMarketplaceService>();
            mockService
                .Setup(service => service.FilterByCategory(It.IsAny<IEnumerable<Equipment>>(), It.IsAny<string?>()))
                .Returns((IEnumerable<Equipment> items, string? category) =>
                    string.IsNullOrEmpty(category) || category == "All"
                        ? items.ToList()
                        : items.Where(item => item.Category == category).ToList());
        }

        [Fact]
        public void Constructor_WithData_LoadsItems()
        {
            var items = CreateMixedEquipmentList();
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(items);

            var viewModel = new MarketplaceViewModel(mockService.Object);

            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void Constructor_EmptyResult_SetsEmptyList()
        {
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(new List<Equipment>());

            var viewModel = new MarketplaceViewModel(mockService.Object);

            Assert.Empty(viewModel.AvailableItems);
        }

        [Fact]
        public void FilterByCategory_All_ReturnsAllItems()
        {
            var items = CreateMixedEquipmentList();
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(mockService.Object);

            viewModel.FilterByCategory("All");

            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void FilterByCategory_Null_ReturnsAllItems()
        {
            var items = CreateMixedEquipmentList();
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(mockService.Object);

            viewModel.FilterByCategory(null);

            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void FilterByCategory_Empty_ReturnsAllItems()
        {
            var items = CreateMixedEquipmentList();
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(mockService.Object);

            viewModel.FilterByCategory(string.Empty);

            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void FilterByCategory_SpecificCategory_ReturnsCorrectCount()
        {
            var items = new List<Equipment>
            {
                new Equipment { ID = CameraItemId, Category = "Camera" },
                new Equipment { ID = LightingItemId, Category = "Lighting" },
                new Equipment { ID = SecondLightingItemId, Category = "Lighting" },
            };
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(mockService.Object);

            viewModel.FilterByCategory("Lighting");

            Assert.Equal(2, viewModel.AvailableItems.Count);
        }

        [Fact]
        public void FilterByCategory_SpecificCategory_ReturnsMatchingItems()
        {
            var items = new List<Equipment>
            {
                new Equipment { ID = CameraItemId, Category = "Camera" },
                new Equipment { ID = LightingItemId, Category = "Lighting" },
                new Equipment { ID = SecondLightingItemId, Category = "Lighting" },
            };
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(items);
            var viewModel = new MarketplaceViewModel(mockService.Object);

            viewModel.FilterByCategory("Lighting");

            Assert.All(viewModel.AvailableItems, item => Assert.Equal("Lighting", item.Category));
        }

        [Fact]
        public void StatusMessage_UserNotLoggedIn_ReturnsLogInMessage()
        {
            SessionManager.CurrentUserID = NotLoggedInUserId;
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(new List<Equipment>());
            var viewModel = new MarketplaceViewModel(mockService.Object);

            Assert.Equal("Please log in to purchase equipment.", viewModel.StatusMessage);
        }

        [Fact]
        public void StatusMessage_UserLoggedIn_ReturnsEmpty()
        {
            SessionManager.CurrentUserID = LoggedInUserId;
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(new List<Equipment>());
            var viewModel = new MarketplaceViewModel(mockService.Object);

            Assert.Empty(viewModel.StatusMessage);
        }

        [Fact]
        public void UserBalance_ReturnsSessionBalance()
        {
            SessionManager.CurrentUserBalance = UserBalance;
            mockService.Setup(service => service.GetAvailableEquipment()).Returns(new List<Equipment>());
            var viewModel = new MarketplaceViewModel(mockService.Object);

            Assert.Equal(UserBalance, viewModel.UserBalance);
        }

        private List<Equipment> CreateMixedEquipmentList()
        {
            return new List<Equipment>
            {
                new Equipment { ID = CameraItemId, Category = "Camera" },
                new Equipment { ID = LightingItemId, Category = "Lighting" },
            };
        }
    }
}
