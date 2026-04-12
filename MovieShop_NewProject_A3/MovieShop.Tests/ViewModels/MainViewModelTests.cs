using Moq;
using MovieShop.Models;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private const int NotLoggedInUserId = 0;
        private const decimal ZeroBalance = 0m;
        private const decimal InitialBalance = 100m;
        private const decimal UpdatedBalance = 200m;
        private const decimal RefreshedBalance = 300m;
        private const decimal DisplayBalanceAmount = 150.50m;

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
        public void Constructor_ValidUser_SetsBalance()
        {
            mockWalletService.Setup(service => service.GetBalance(SessionManager.DefaultUserID)).Returns(InitialBalance);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());

            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            Assert.Equal(InitialBalance, viewModel.Balance);
        }

        [Fact]
        public void Constructor_ValidUser_NavigatesToShop()
        {
            mockWalletService.Setup(service => service.GetBalance(SessionManager.DefaultUserID)).Returns(InitialBalance);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());

            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            Assert.Equal("Shop", viewModel.CurrentViewModel);
        }

        [Fact]
        public void Constructor_ValidUser_InitializesFlashSaleVM()
        {
            mockWalletService.Setup(service => service.GetBalance(SessionManager.DefaultUserID)).Returns(InitialBalance);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());

            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            Assert.NotNull(viewModel.FlashSaleVM);
        }

        [Fact]
        public void Constructor_InvalidUser_SetsBalanceToZero()
        {
            SessionManager.CurrentUserID = NotLoggedInUserId;
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());

            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            Assert.Equal(ZeroBalance, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_ValidUser_UpdatesBalance()
        {
            SessionManager.CurrentUserID = SessionManager.DefaultUserID;
            mockWalletService.Setup(service => service.GetBalance(SessionManager.DefaultUserID)).Returns(InitialBalance);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            mockWalletService.Setup(service => service.GetBalance(SessionManager.DefaultUserID)).Returns(UpdatedBalance);
            viewModel.RefreshBalanceFromDatabase();

            Assert.Equal(UpdatedBalance, viewModel.Balance);
        }

        [Fact]
        public void RefreshBalanceFromDatabase_InvalidUser_SetsBalanceToZero()
        {
            SessionManager.CurrentUserID = NotLoggedInUserId;
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            viewModel.RefreshBalanceFromDatabase();

            Assert.Equal(ZeroBalance, viewModel.Balance);
        }

        [Fact]
        public void NavigateToMarketplaceCommand_SetsCurrentViewModel()
        {
            var viewModel = CreateDefaultViewModel();

            viewModel.NavigateToMarketplaceCommand.Execute(null);

            Assert.Equal("Marketplace", viewModel.CurrentViewModel);
        }

        [Fact]
        public void NavigateToInventoryCommand_SetsCurrentViewModel()
        {
            var viewModel = CreateDefaultViewModel();

            viewModel.NavigateToInventoryCommand.Execute(null);

            Assert.Equal("Inventory", viewModel.CurrentViewModel);
        }

        [Fact]
        public void NavigateToTicketsCommand_SetsCurrentViewModel()
        {
            var viewModel = CreateDefaultViewModel();

            viewModel.NavigateToTicketsCommand.Execute(null);

            Assert.Equal("Tickets", viewModel.CurrentViewModel);
        }

        [Fact]
        public void NavigateToShopCommand_SetsCurrentViewModel()
        {
            var viewModel = CreateDefaultViewModel();

            viewModel.NavigateToShopCommand.Execute(null);

            Assert.Equal("Shop", viewModel.CurrentViewModel);
        }

        [Fact]
        public void NavigateToWalletCommand_SetsWalletViewModel()
        {
            var viewModel = CreateDefaultViewModel();

            viewModel.NavigateToWalletCommand.Execute(null);

            Assert.IsType<WalletViewModel>(viewModel.CurrentViewModel);
        }

        [Fact]
        public void RefreshWallet_UpdatesBalance()
        {
            mockWalletService.Setup(service => service.GetBalance(It.IsAny<int>())).Returns(InitialBalance);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());
            mockWalletService.Setup(service => service.GetTransactionsByUserId(It.IsAny<int>())).Returns(new List<Transaction>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            mockWalletService.Setup(service => service.GetBalance(It.IsAny<int>())).Returns(RefreshedBalance);
            viewModel.RefreshWallet();

            Assert.Equal(RefreshedBalance, viewModel.Balance);
        }

        [Fact]
        public void RefreshWallet_ReloadsTransactions()
        {
            mockWalletService.Setup(service => service.GetBalance(It.IsAny<int>())).Returns(InitialBalance);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());
            mockWalletService.Setup(service => service.GetTransactionsByUserId(It.IsAny<int>())).Returns(new List<Transaction>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            viewModel.RefreshWallet();

            mockWalletService.Verify(service => service.GetTransactionsByUserId(It.IsAny<int>()), Times.AtLeastOnce);
        }

        [Fact]
        public void DisplayBalance_ReturnsFormattedCurrency()
        {
            mockWalletService.Setup(service => service.GetBalance(It.IsAny<int>())).Returns(DisplayBalanceAmount);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());
            var viewModel = new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);

            Assert.Equal(DisplayBalanceAmount.ToString("C"), viewModel.DisplayBalance);
        }

        private MainViewModel CreateDefaultViewModel()
        {
            mockWalletService.Setup(service => service.GetBalance(It.IsAny<int>())).Returns(InitialBalance);
            mockActiveSalesService.Setup(service => service.GetCurrentSales()).Returns(new List<ActiveSale>());
            return new MainViewModel(mockWalletService.Object, mockActiveSalesService.Object, mockSaleService.Object);
        }
    }
}
