using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class WalletViewModelTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;

        public WalletViewModelTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();
        }

        [Fact]
        public void Constructor_InitializesValues()
        {
            // Arrange & Act
            var viewModel = new WalletViewModel(1, 100m, _mockUserRepo.Object, _mockTransactionRepo.Object);

            // Assert
            Assert.Equal(100m, viewModel.Balance);
            Assert.Equal("$100.00", viewModel.DisplayBalance);
            Assert.NotNull(viewModel.Transactions);
            Assert.Empty(viewModel.Transactions);
        }

        [Fact]
        public async Task LoadTransactionsAsync_PopulatesTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { ID = 1, Amount = 50, Type = "TopUp" }
            };
            _mockTransactionRepo.Setup(r => r.GetTransactionsByUserId(1)).Returns(transactions);
            var viewModel = new WalletViewModel(1, 100m, _mockUserRepo.Object, _mockTransactionRepo.Object);

            // Act
            await viewModel.LoadTransactionsAsync();

            // Assert
            Assert.Single(viewModel.Transactions);
            Assert.Equal(50, viewModel.Transactions[0].Amount);
            Assert.False(viewModel.IsLoading);
            Assert.Empty(viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadTransactionsAsync_HandlesException()
        {
            // Arrange
            _mockTransactionRepo.Setup(r => r.GetTransactionsByUserId(1)).Throws(new Exception("DB Error"));
            var viewModel = new WalletViewModel(1, 100m, _mockUserRepo.Object, _mockTransactionRepo.Object);

            // Act
            await viewModel.LoadTransactionsAsync();

            // Assert
            Assert.Contains("Failed to load", viewModel.ErrorMessage);
            Assert.False(viewModel.IsLoading);
        }

        [Theory]
        [InlineData("", "1234123412341234", "12/26", "123", 50, "Please enter the cardholder name.")]
        [InlineData("John123", "1234123412341234", "12/26", "123", 50, "Cardholder name can only contain letters and spaces.")]
        [InlineData("John ", "1234123412341234", "12/26", "123", 50, "Please enter both first and last name.")]
        [InlineData("J D", "1234123412341234", "12/26", "123", 50, "Each name must be at least 2 characters long.")]
        [InlineData("John Doe", "1234", "12/26", "123", 50, "Card number must be exactly 16 digits.")]
        [InlineData("John Doe", "1234123412341234", "invalid", "123", 50, "Invalid expiration date")]
        [InlineData("John Doe", "1234123412341234", "01/20", "123", 50, "Your card has expired")] // Past date
        [InlineData("John Doe", "1234123412341234", "12/35", "12", 50, "CVV must be exactly 3 digits.")]
        [InlineData("John Doe", "1234123412341234", "12/35", "123", 0, "Amount must be greater than 0.")]
        public void TopUp_InvalidInput_SetsErrorMessage(string name, string card, string exp, string cvv, double amount, string expectedError)
        {
            // Arrange
            var viewModel = new WalletViewModel(1, 100m, _mockUserRepo.Object, _mockTransactionRepo.Object)
            {
                CardHolderName = name,
                CardNumber = card,
                ExpirationDate = exp,
                CVV = cvv,
                TopUpAmount = amount
            };

            // Act
            viewModel.TopUpCommand.Execute(null);

            // Assert
            Assert.Contains(expectedError, viewModel.ErrorMessage);
            Assert.Empty(viewModel.SuccessMessage);
            _mockUserRepo.Verify(r => r.UpdateBalance(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public void TopUp_ValidInput_UpdatesBalanceAndLogsTransaction()
        {
            // Arrange
            var viewModel = new WalletViewModel(1, 100m, _mockUserRepo.Object, _mockTransactionRepo.Object)
            {
                CardHolderName = "John Doe",
                CardNumber = "1234123412341234",
                ExpirationDate = "12/35", // Future date
                CVV = "123",
                TopUpAmount = 50
            };

            // Act
            viewModel.TopUpCommand.Execute(null);

            // Assert
            Assert.Empty(viewModel.ErrorMessage);
            Assert.Contains("Successfully added", viewModel.SuccessMessage);
            Assert.Equal(150m, viewModel.Balance);
            _mockUserRepo.Verify(r => r.UpdateBalance(1, 150m), Times.Once);
            
            // Note: LogTopUpTransaction is async without await, so we might need a small delay or just wait until background tasks finish
            // Task.Delay(100).Wait(); 
            // Assert.Single(viewModel.Transactions); 
            // It inserts 0 immediately to observable collection, then tasks repo
            Assert.Single(viewModel.Transactions);
            Assert.Equal(50m, viewModel.Transactions[0].Amount);
            Assert.Equal("TopUp", viewModel.Transactions[0].Type);
        }

        [Fact]
        public void OnTransactionCompleted_UpdatesBalanceAndReloads()
        {
            // Arrange
            _mockTransactionRepo.Setup(r => r.GetTransactionsByUserId(1)).Returns(new List<Transaction>());
            var viewModel = new WalletViewModel(1, 100m, _mockUserRepo.Object, _mockTransactionRepo.Object);

            // Act
            viewModel.OnTransactionCompleted(25m);

            // Assert
            Assert.Equal(125m, viewModel.Balance);
            _mockUserRepo.Verify(r => r.UpdateBalance(1, 125m), Times.Once);
        }
    }
}
