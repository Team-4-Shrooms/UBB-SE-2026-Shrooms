using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class WalletViewModelTests
    {
        private const int ValidUserId = 1;
        private const decimal InitialBalance = 100m;
        private const decimal TopUpAmount = 50m;
        private const decimal BalanceAfterTopUp = 150m;
        private const decimal TransactionCompletedAmount = 25m;
        private const decimal BalanceAfterTransaction = 125m;
        private const int TransactionId = 1;
        private const string ValidCardHolder = "John Doe";
        private const string ValidCardNumber = "1234123412341234";
        private const string ValidExpiration = "12/35";
        private const string ValidCvv = "123";

        private readonly Mock<IWalletService> mockWalletService;

        public WalletViewModelTests()
        {
            mockWalletService = new Mock<IWalletService>();
        }

        [Fact]
        public void Constructor_ValidInput_SetsBalance()
        {
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            Assert.Equal(InitialBalance, viewModel.Balance);
        }

        [Fact]
        public void Constructor_ValidInput_SetsDisplayBalance()
        {
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            Assert.Equal("$100.00", viewModel.DisplayBalance);
        }

        [Fact]
        public void Constructor_ValidInput_InitializesEmptyTransactions()
        {
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            Assert.Empty(viewModel.Transactions);
        }

        [Fact]
        public async Task LoadTransactionsAsync_WithData_PopulatesTransactions()
        {
            var transactions = new List<Transaction>
            {
                new Transaction { ID = TransactionId, Amount = TopUpAmount, Type = "TopUp" },
            };
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Returns(transactions);
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            await viewModel.LoadTransactionsAsync();

            Assert.Single(viewModel.Transactions);
        }

        [Fact]
        public async Task LoadTransactionsAsync_WithData_SetsCorrectAmount()
        {
            var transactions = new List<Transaction>
            {
                new Transaction { ID = TransactionId, Amount = TopUpAmount, Type = "TopUp" },
            };
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Returns(transactions);
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            await viewModel.LoadTransactionsAsync();

            Assert.Equal(TopUpAmount, viewModel.Transactions[0].Amount);
        }

        [Fact]
        public async Task LoadTransactionsAsync_WithData_ClearsLoadingState()
        {
            var transactions = new List<Transaction>
            {
                new Transaction { ID = TransactionId, Amount = TopUpAmount, Type = "TopUp" },
            };
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Returns(transactions);
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            await viewModel.LoadTransactionsAsync();

            Assert.False(viewModel.IsLoading);
        }

        [Fact]
        public async Task LoadTransactionsAsync_WithData_ClearsErrorMessage()
        {
            var transactions = new List<Transaction>
            {
                new Transaction { ID = TransactionId, Amount = TopUpAmount, Type = "TopUp" },
            };
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Returns(transactions);
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            await viewModel.LoadTransactionsAsync();

            Assert.Empty(viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadTransactionsAsync_Exception_SetsErrorMessage()
        {
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Throws(new Exception("DB Error"));
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            await viewModel.LoadTransactionsAsync();

            Assert.Contains("Failed to load", viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoadTransactionsAsync_Exception_ClearsLoadingState()
        {
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Throws(new Exception("DB Error"));
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            await viewModel.LoadTransactionsAsync();

            Assert.False(viewModel.IsLoading);
        }

        [Theory]
        [InlineData("", "1234123412341234", "12/26", "123", 50, "Please enter the cardholder name.")]
        [InlineData("John123", "1234123412341234", "12/26", "123", 50, "Cardholder name can only contain letters and spaces.")]
        [InlineData("John ", "1234123412341234", "12/26", "123", 50, "Please enter both first and last name.")]
        [InlineData("J D", "1234123412341234", "12/26", "123", 50, "Each name must be at least 2 characters long.")]
        [InlineData("John Doe", "1234", "12/26", "123", 50, "Card number must be exactly 16 digits.")]
        [InlineData("John Doe", "1234123412341234", "invalid", "123", 50, "Invalid expiration date")]
        [InlineData("John Doe", "1234123412341234", "01/20", "123", 50, "Your card has expired")]
        [InlineData("John Doe", "1234123412341234", "12/35", "12", 50, "CVV must be exactly 3 digits.")]
        [InlineData("John Doe", "1234123412341234", "12/35", "123", 0, "Amount must be greater than 0.")]
        public void TopUp_InvalidInput_SetsErrorMessage(string name, string card, string exp, string cvv, double amount, string expectedError)
        {
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object)
            {
                CardHolderName = name,
                CardNumber = card,
                ExpirationDate = exp,
                CVV = cvv,
                TopUpAmount = amount,
            };

            viewModel.TopUpCommand.Execute(null);

            Assert.Contains(expectedError, viewModel.ErrorMessage);
        }

        [Theory]
        [InlineData("", "1234123412341234", "12/26", "123", 50)]
        [InlineData("John123", "1234123412341234", "12/26", "123", 50)]
        [InlineData("John ", "1234123412341234", "12/26", "123", 50)]
        [InlineData("J D", "1234123412341234", "12/26", "123", 50)]
        [InlineData("John Doe", "1234", "12/26", "123", 50)]
        [InlineData("John Doe", "1234123412341234", "invalid", "123", 50)]
        [InlineData("John Doe", "1234123412341234", "01/20", "123", 50)]
        [InlineData("John Doe", "1234123412341234", "12/35", "12", 50)]
        [InlineData("John Doe", "1234123412341234", "12/35", "123", 0)]
        public void TopUp_InvalidInput_DoesNotUpdateBalance(string name, string card, string exp, string cvv, double amount)
        {
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object)
            {
                CardHolderName = name,
                CardNumber = card,
                ExpirationDate = exp,
                CVV = cvv,
                TopUpAmount = amount,
            };

            viewModel.TopUpCommand.Execute(null);

            mockWalletService.Verify(service => service.UpdateBalance(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public void TopUp_ValidInput_UpdatesBalance()
        {
            var viewModel = CreateValidTopUpViewModel();

            viewModel.TopUpCommand.Execute(null);

            Assert.Equal(BalanceAfterTopUp, viewModel.Balance);
        }

        [Fact]
        public void TopUp_ValidInput_SetsSuccessMessage()
        {
            var viewModel = CreateValidTopUpViewModel();

            viewModel.TopUpCommand.Execute(null);

            Assert.Contains("Successfully added", viewModel.SuccessMessage);
        }

        [Fact]
        public void TopUp_ValidInput_ClearsErrorMessage()
        {
            var viewModel = CreateValidTopUpViewModel();

            viewModel.TopUpCommand.Execute(null);

            Assert.Empty(viewModel.ErrorMessage);
        }

        [Fact]
        public void TopUp_ValidInput_CallsUpdateBalance()
        {
            var viewModel = CreateValidTopUpViewModel();

            viewModel.TopUpCommand.Execute(null);

            mockWalletService.Verify(service => service.UpdateBalance(ValidUserId, BalanceAfterTopUp), Times.Once);
        }

        [Fact]
        public void TopUp_ValidInput_AddsTransaction()
        {
            var viewModel = CreateValidTopUpViewModel();

            viewModel.TopUpCommand.Execute(null);

            Assert.Single(viewModel.Transactions);
        }

        [Fact]
        public void TopUp_ValidInput_AddsCorrectTransactionAmount()
        {
            var viewModel = CreateValidTopUpViewModel();

            viewModel.TopUpCommand.Execute(null);

            Assert.Equal(TopUpAmount, viewModel.Transactions[0].Amount);
        }

        [Fact]
        public void TopUp_ValidInput_AddsTopUpTransactionType()
        {
            var viewModel = CreateValidTopUpViewModel();

            viewModel.TopUpCommand.Execute(null);

            Assert.Equal("TopUp", viewModel.Transactions[0].Type);
        }

        [Fact]
        public void OnTransactionCompleted_ValidAmount_UpdatesBalance()
        {
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Returns(new List<Transaction>());
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            viewModel.OnTransactionCompleted(TransactionCompletedAmount);

            Assert.Equal(BalanceAfterTransaction, viewModel.Balance);
        }

        [Fact]
        public void OnTransactionCompleted_ValidAmount_CallsUpdateBalance()
        {
            mockWalletService.Setup(service => service.GetTransactionsByUserId(ValidUserId)).Returns(new List<Transaction>());
            var viewModel = new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object);

            viewModel.OnTransactionCompleted(TransactionCompletedAmount);

            mockWalletService.Verify(service => service.UpdateBalance(ValidUserId, BalanceAfterTransaction), Times.Once);
        }

        private WalletViewModel CreateValidTopUpViewModel()
        {
            return new WalletViewModel(ValidUserId, InitialBalance, mockWalletService.Object)
            {
                CardHolderName = ValidCardHolder,
                CardNumber = ValidCardNumber,
                ExpirationDate = ValidExpiration,
                CVV = ValidCvv,
                TopUpAmount = (double)TopUpAmount,
            };
        }
    }
}
