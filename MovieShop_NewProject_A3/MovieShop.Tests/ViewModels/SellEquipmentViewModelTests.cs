using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class SellEquipmentViewModelTests
    {
        private const int ValidUserId = 1;
        private const decimal ValidPrice = 10.50m;
        private const decimal SubmitPrice = 100m;

        private readonly Mock<IMarketplaceService> mockMarketplaceService;

        public SellEquipmentViewModelTests()
        {
            mockMarketplaceService = new Mock<IMarketplaceService>();
            SessionManager.CurrentUserID = ValidUserId;
        }

        [Theory]
        [InlineData("Valid Title", "10.50", true)]
        [InlineData("", "10.50", false)]
        [InlineData("Title", "abc", false)]
        [InlineData("Title", "-5", false)]
        [InlineData("Title", "0", false)]
        public void ValidateForm_VariousInputs_SetsCanPost(string title, string priceInput, bool expectedCanPost)
        {
            var viewModel = new SellEquipmentViewModel(mockMarketplaceService.Object);

            viewModel.NewItemTitle = title;
            viewModel.PriceInput = priceInput;

            Assert.Equal(expectedCanPost, viewModel.CanPost);
        }

        [Theory]
        [InlineData("Title", "abc", "Please enter a valid numeric price!")]
        [InlineData("Title", "-5", "Price must be greater than 0!")]
        [InlineData("Title", "0", "Price must be greater than 0!")]
        public void ValidateForm_InvalidPrice_SetsErrorMessage(string title, string priceInput, string expectedError)
        {
            var viewModel = new SellEquipmentViewModel(mockMarketplaceService.Object);

            viewModel.NewItemTitle = title;
            viewModel.PriceInput = priceInput;

            Assert.Equal(expectedError, viewModel.PriceErrorMessage);
        }

        [Fact]
        public void ValidateForm_ValidInput_ClearsErrorMessage()
        {
            var viewModel = new SellEquipmentViewModel(mockMarketplaceService.Object);

            viewModel.NewItemTitle = "Valid Title";
            viewModel.PriceInput = "10.50";

            Assert.Empty(viewModel.PriceErrorMessage);
        }

        [Fact]
        public void ValidateForm_ValidInput_SetsValidatedPrice()
        {
            var viewModel = new SellEquipmentViewModel(mockMarketplaceService.Object);

            viewModel.NewItemTitle = "Valid Title";
            viewModel.PriceInput = "10.50";

            Assert.Equal(ValidPrice, viewModel.ValidatedPrice);
        }

        [Fact]
        public void SubmitListing_CanPost_SavesItemToRepository()
        {
            var viewModel = new SellEquipmentViewModel(mockMarketplaceService.Object)
            {
                NewItemTitle = "Camera",
                NewItemDesc = "Great Camera",
                PriceInput = "100",
            };

            viewModel.SubmitListing("Electronics", "New", "image.png");

            mockMarketplaceService.Verify(service => service.ListItem(It.Is<Equipment>(equipment =>
                equipment.Title == "Camera" &&
                equipment.Description == "Great Camera" &&
                equipment.Price == SubmitPrice &&
                equipment.Category == "Electronics" &&
                equipment.Condition == "New" &&
                equipment.ImageUrl == "image.png" &&
                equipment.Status == EquipmentStatus.Available &&
                equipment.SellerID == ValidUserId)), Times.Once);
        }
    }
}
