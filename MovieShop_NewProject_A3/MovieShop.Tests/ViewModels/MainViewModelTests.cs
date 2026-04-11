using System.ComponentModel;
using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private readonly Mock<IWalletService> mockWalletService;
        private readonly Mock<IActiveSalesService> mockActiveSalesService;
        private readonly Mock<ISaleService> mockSaleService;

        public MainViewModelTests()
        {
            mockWalletService = new Mock<IWalletService>();
            mockActiveSalesService = new Mock<IActiveSalesService>();
            mockSaleService = new Mock<ISaleService>();

            SessionManager.CurrentUserID = SessionManager.DefaultUserID;
        }

        [Fact]
        public void Constructor_ValidUser_SetsBalanceAndNavigatesToShop()
        {
            // Arrange
            mockWalletService.Setup(s => s.GetBalance(SessionManager.DefaultUserID)).Returns(100m);
            mockActiveSalesService.Setup(s => s.GetCurrentSales()).Returns(new List<ActiveSale>());

            // Act
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

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
            mockActiveSalesService.Setup(s => s.GetCurrentSales()).Returns(new List<ActiveSale>());

            // Act
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            // Assert
            Assert.Equal(0m, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_ValidUser_UpdatesBalance()
        {
            // Arrange
            SessionManager.CurrentUserID = SessionManager.DefaultUserID;
            mockWalletService.Setup(s => s.GetBalance(SessionManager.DefaultUserID)).Returns(150m);
            mockActiveSalesService.Setup(s => s.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            // Act
            mockWalletService.Setup(s => s.GetBalance(SessionManager.DefaultUserID)).Returns(200m);
            viewModel.RefreshBalanceFromDatabase();

            // Assert
            Assert.Equal(200m, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_InvalidUser_SetsBalanceToZero()
        {
            // Arrange — currentUserID is captured at construction, so set session before ctor
            SessionManager.CurrentUserID = 0;
            mockActiveSalesService.Setup(s => s.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            // Act
            viewModel.RefreshBalanceFromDatabase();

            // Assert
            Assert.Equal(0m, viewModel.Balance);
        }

        [Fact]
        public void Commands_Navigation_SetCurrentViewModel()
        {
            // Arrange
            mockWalletService.Setup(s => s.GetBalance(It.IsAny<int>())).Returns(100m);
            mockActiveSalesService.Setup(s => s.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

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
            mockWalletService.Setup(s => s.GetBalance(It.IsAny<int>())).Returns(100m);
            mockActiveSalesService.Setup(s => s.GetCurrentSales()).Returns(new List<ActiveSale>());
            mockWalletService.Setup(s => s.GetTransactionsByUserId(It.IsAny<int>())).Returns(new List<Transaction>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            // Act
            mockWalletService.Setup(s => s.GetBalance(It.IsAny<int>())).Returns(300m);
            viewModel.RefreshWallet();

            // Assert
            Assert.Equal(300m, viewModel.Balance);
            mockWalletService.Verify(s => s.GetTransactionsByUserId(It.IsAny<int>()), Times.AtLeastOnce);
        }

        [Fact]
        public void DisplayBalance_ReturnsFormattedCurrency()
        {
            // Arrange
            mockWalletService.Setup(s => s.GetBalance(It.IsAny<int>())).Returns(150.50m);
            mockActiveSalesService.Setup(s => s.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            // Act
            var display = viewModel.DisplayBalance;

            // Assert
            Assert.Equal(150.50m.ToString("C"), display);
        }
    }
}
