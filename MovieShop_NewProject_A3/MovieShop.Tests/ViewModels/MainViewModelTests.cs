using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.ViewModels;
using System.ComponentModel;

namespace MovieShop.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IActiveSalesRepository> _mockSalesRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;

        public MainViewModelTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockSalesRepo = new Mock<IActiveSalesRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();

            SessionManager.CurrentUserID = SessionManager.DefaultUserID;
        }

        [Fact]
        public void Constructor_ValidUser_SetsBalanceAndNavigatesToShop()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(100m);
            _mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());

            // Act
            var viewModel = new MainViewModel(_mockUserRepo.Object, _mockSalesRepo.Object, _mockTransactionRepo.Object);

            // Assert
            Assert.Equal(100m, viewModel.Balance);
            Assert.Equal("Shop", viewModel.CurrentViewModel);
            Assert.NotNull(viewModel.FlashSaleVM);
        }

        [Fact]
        public void Constructor_InvalidUser_SetsBalanceToZero()
        {
            // Arrange
            SessionManager.CurrentUserID = 0;
            _mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());

            // Act
            var viewModel = new MainViewModel(_mockUserRepo.Object, _mockSalesRepo.Object, _mockTransactionRepo.Object);

            // Assert
            Assert.Equal(0m, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_ValidUser_UpdatesBalance()
        {
            // Arrange
            SessionManager.CurrentUserID = SessionManager.DefaultUserID;
            _mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(150m);
            _mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(_mockUserRepo.Object, _mockSalesRepo.Object, _mockTransactionRepo.Object);

            // Act
            _mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(200m);
            viewModel.RefreshBalanceFromDatabase();

            // Assert
            Assert.Equal(200m, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_InvalidUser_SetsBalanceToZero()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(150m);
            _mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(_mockUserRepo.Object, _mockSalesRepo.Object, _mockTransactionRepo.Object);

            // Act
            SessionManager.CurrentUserID = 0;
            viewModel.RefreshBalanceFromDatabase();

            // Assert
            Assert.Equal(0m, viewModel.Balance);
        }

        [Fact]
        public void Commands_Navigation_SetCurrentViewModel()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(100m);
            _mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(_mockUserRepo.Object, _mockSalesRepo.Object, _mockTransactionRepo.Object);

            // Act & Assert
            viewModel.NavigateToMarketplaceCommand.Execute(null);
            Assert.Equal("Marketplace", viewModel.CurrentViewModel);

            viewModel.NavigateToInventoryCommand.Execute(null);
            Assert.Equal("Inventory", viewModel.CurrentViewModel);

            viewModel.NavigateToTicketsCommand.Execute(null);
            Assert.Equal("Tickets", viewModel.CurrentViewModel);

            viewModel.NavigateToShopCommand.Execute(null);
            Assert.Equal("Shop", viewModel.CurrentViewModel);

            viewModel.NavigateToWalletCommand.Execute(null);
            Assert.IsType<WalletViewModel>(viewModel.CurrentViewModel);
        }

        [Fact]
        public void RefreshWallet_UpdatesBalanceAndTransactions()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(100m);
            _mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            _mockTransactionRepo.Setup(repo => repo.GetTransactionsByUserId(It.IsAny<int>())).Returns(new List<Transaction>());
            var viewModel = new MainViewModel(_mockUserRepo.Object, _mockSalesRepo.Object, _mockTransactionRepo.Object);
            
            // Act
            _mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(300m);
            viewModel.RefreshWallet();

            // Assert
            Assert.Equal(300m, viewModel.Balance);
            _mockTransactionRepo.Verify(repo => repo.GetTransactionsByUserId(It.IsAny<int>()), Times.AtLeastOnce);
        }
        
        [Fact]
        public void DisplayBalance_ReturnsFormattedCurrency()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(150.50m);
            _mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(_mockUserRepo.Object, _mockSalesRepo.Object, _mockTransactionRepo.Object);

            // Act
            var display = viewModel.DisplayBalance;

            // Assert
            Assert.Equal(150.50m.ToString("C"), display);
        }
    }
}
