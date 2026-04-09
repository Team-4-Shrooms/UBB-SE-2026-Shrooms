using System;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class ConfirmReceiptViewModelTests
    {
        private readonly Mock<IUserRepository> mockUserRepo;
        private readonly Mock<IServiceProvider> mockServiceProvider;

        public ConfirmReceiptViewModelTests()
        {
            mockUserRepo = new Mock<IUserRepository>();

            mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(x => x.GetService(typeof(IUserRepository))).Returns(mockUserRepo.Object);

            TestServiceHelper.SetAppServices(mockServiceProvider.Object);
        }

        [Fact]
        public void ConfirmReceipt_ValidTransaction_ReleasesEscrowAndUpdatesStatus()
        {
            // Arrange
            var transaction = new Transaction
            {
                Status = "Pending",
                Amount = 100m,
                BuyerID = new User { ID = 1 },
                SellerID = new User { ID = 2 }
            };

            mockUserRepo.Setup(r => r.GetBalance(2)).Returns(50m);
            var viewModel = new ConfirmReceiptViewModel(1, transaction, 50m);

            // Act
            viewModel.ConfirmReceiptCommand.Execute(null);

            // Assert
            Assert.Equal("Completed", transaction.Status);
            Assert.True(viewModel.IsSuccessVisible);
            Assert.False(viewModel.IsConfirmButtonVisible);
            Assert.Contains("Receipt confirmed", viewModel.SuccessMessage);
            Assert.Equal(150m, viewModel.SellerBalance);
            mockUserRepo.Verify(r => r.UpdateBalance(2, 150m), Times.Once);
        }

        [Fact]
        public void ConfirmReceipt_NotBuyer_SetsErrorMessage()
        {
            // Arrange
            var transaction = new Transaction
            {
                Status = "Pending",
                BuyerID = new User { ID = 3 },
            };

            var viewModel = new ConfirmReceiptViewModel(1, transaction, 50m);

            // Act
            viewModel.ConfirmReceiptCommand.Execute(null);

            // Assert
            Assert.Contains("Only the buyer", viewModel.ErrorMessage);
            Assert.Equal("Pending", transaction.Status);
        }

        [Fact]
        public void ConfirmReceipt_TransactionNotPending_SetsErrorMessage()
        {
            // Arrange
            var transaction = new Transaction
            {
                Status = "Completed",
                BuyerID = new User { ID = 1 },
            };

            var viewModel = new ConfirmReceiptViewModel(1, transaction, 50m);

            // Act
            viewModel.ConfirmReceiptCommand.Execute(null);

            // Assert
            Assert.Contains("already been completed", viewModel.ErrorMessage);
        }

        [Fact]
        public void ConfirmReceipt_NoTransaction_SetsErrorMessage()
        {
            // Arrange
            var viewModel = new ConfirmReceiptViewModel(1, null, 50m);

            // Act
            viewModel.ConfirmReceiptCommand.Execute(null);

            // Assert
            Assert.Contains("No transaction found", viewModel.ErrorMessage);
        }

        [Fact]
        public void DismissSuccess_HidesSuccessMessage()
        {
            // Arrange
            var transaction = new Transaction
            {
                Status = "Completed",
                BuyerID = new User { ID = 1 },
            };
            var viewModel = new ConfirmReceiptViewModel(1, transaction, 50m)
            {
                IsSuccessVisible = true,
                SuccessMessage = "Test"
            };

            // Act
            viewModel.DismissSuccessCommand.Execute(null);

            // Assert
            Assert.False(viewModel.IsSuccessVisible);
            Assert.Empty(viewModel.SuccessMessage);
        }
    }
}
