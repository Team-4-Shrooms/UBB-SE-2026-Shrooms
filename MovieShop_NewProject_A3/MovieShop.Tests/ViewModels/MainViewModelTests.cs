using System.ComponentModel;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private readonly Mock<IUserRepository> mockUserRepo;
        private readonly Mock<IActiveSalesRepository> mockSalesRepo;
        private readonly Mock<ITransactionRepository> mockTransactionRepo;
        private readonly Mock<ISaleService> mockSaleService;

        public MainViewModelTests()
        {
            mockUserRepo = new Mock<IUserRepository>();
            mockSalesRepo = new Mock<IActiveSalesRepository>();
            mockTransactionRepo = new Mock<ITransactionRepository>();
            mockSaleService = new Mock<ISaleService>();

            SessionManager.CurrentUserID = SessionManager.DefaultUserID;
        }

        [Fact]
        public void Constructor_ValidUser_SetsBalanceAndNavigatesToShop()
        {
            // Arrange
            mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(100m);
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());

            // Act
            var viewModel = new MainViewModel(mockUserRepo.Object, mockSalesRepo.Object, mockTransactionRepo.Object, mockSaleService.Object);

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
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());

            // Act
            var viewModel = new MainViewModel(mockUserRepo.Object, mockSalesRepo.Object, mockTransactionRepo.Object, mockSaleService.Object);

            // Assert
            Assert.Equal(0m, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_ValidUser_UpdatesBalance()
        {
            // Arrange
            SessionManager.CurrentUserID = SessionManager.DefaultUserID;
            mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(150m);
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockUserRepo.Object, mockSalesRepo.Object, mockTransactionRepo.Object, mockSaleService.Object);

            // Act
            mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(200m);
            viewModel.RefreshBalanceFromDatabase();

            // Assert
            Assert.Equal(200m, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_InvalidUser_SetsBalanceToZero()
        {
            // Arrange
            mockUserRepo.Setup(repo => repo.GetBalance(SessionManager.DefaultUserID)).Returns(150m);
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockUserRepo.Object, mockSalesRepo.Object, mockTransactionRepo.Object, mockSaleService.Object);

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
            mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(100m);
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockUserRepo.Object, mockSalesRepo.Object, mockTransactionRepo.Object, mockSaleService.Object);

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
            mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(100m);
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            mockTransactionRepo.Setup(repo => repo.GetTransactionsByUserId(It.IsAny<int>())).Returns(new List<Transaction>());
            var viewModel = new MainViewModel(mockUserRepo.Object, mockSalesRepo.Object, mockTransactionRepo.Object, mockSaleService.Object);

            // Act
            mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(300m);
            viewModel.RefreshWallet();

            // Assert
            Assert.Equal(300m, viewModel.Balance);
            mockTransactionRepo.Verify(repo => repo.GetTransactionsByUserId(It.IsAny<int>()), Times.AtLeastOnce);
        }

        [Fact]
        public void DisplayBalance_ReturnsFormattedCurrency()
        {
            // Arrange
            mockUserRepo.Setup(repo => repo.GetBalance(It.IsAny<int>())).Returns(150.50m);
            mockSalesRepo.Setup(repo => repo.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockUserRepo.Object, mockSalesRepo.Object, mockTransactionRepo.Object, mockSaleService.Object);

            // Act
            var display = viewModel.DisplayBalance;

            // Assert
            Assert.Equal(150.50m.ToString("C"), display);
        }
    }
}
