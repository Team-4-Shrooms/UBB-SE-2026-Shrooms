using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class SellEquipmentViewModelTests
    {
        private readonly Mock<IMarketplaceService> mockMarketplaceService;

        public SellEquipmentViewModelTests()
        {
            mockMarketplaceService = new Mock<IMarketplaceService>();
            SessionManager.CurrentUserID = 1;
        }

        [Theory]
        [InlineData("Valid Title", "10.50", true, 10.50, "")]
        [InlineData("", "10.50", false, 0, "")]
        [InlineData("Title", "abc", false, 0, "Please enter a valid numeric price!")]
        [InlineData("Title", "-5", false, 0, "Price must be greater than 0!")]
        [InlineData("Title", "0", false, 0, "Price must be greater than 0!")]
        public void ValidateForm_VariousInputs_UpdatesCanPostAndErrors(string title, string priceInput, bool expectedCanPost, decimal expectedValidatedPrice, string expectedError)
        {
            // Arrange
            var viewModel = new SellEquipmentViewModel(mockMarketplaceService.Object);

            viewModel.NewItemTitle = title;
            viewModel.PriceInput = priceInput;

            // Assert
            Assert.Equal(expectedCanPost, viewModel.CanPost);
            if (expectedCanPost)
            {
                Assert.Equal(expectedValidatedPrice, viewModel.ValidatedPrice);
                Assert.Empty(viewModel.PriceErrorMessage);
            }
            else
            {
                Assert.Equal(expectedError, viewModel.PriceErrorMessage);
            }
        }

        [Fact]
        public void SubmitListing_CanPost_SavesItemToRepository()
        {
            // Arrange
            var viewModel = new SellEquipmentViewModel(mockMarketplaceService.Object)
            {
                NewItemTitle = "Camera",
                NewItemDesc = "Great Camera",
                PriceInput = "100"
            };

            // Act
            viewModel.SubmitListing("Electronics", "New", "image.png");

            // Assert
            mockMarketplaceService.Verify(s => s.ListItem(It.Is<Equipment>(e =>
                e.Title == "Camera" &&
                e.Description == "Great Camera" &&
                e.Price == 100m &&
                e.Category == "Electronics" &&
                e.Condition == "New" &&
                e.ImageUrl == "image.png" &&
                e.Status == EquipmentStatus.Available &&
                e.SellerID == 1)), Times.Once);
        }
    }
}
