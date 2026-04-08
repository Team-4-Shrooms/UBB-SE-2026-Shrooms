using System;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private object currentViewModel;
        public object CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public FlashSaleViewModel FlashSaleVM { get; set; }
        private readonly IActiveSalesRepository salesRepo;

        private decimal balance;
        public decimal Balance
        {
            get => balance;
            set
            {
                balance = value;
                OnPropertyChanged(nameof(Balance));
                OnPropertyChanged(nameof(DisplayBalance));
            }
        }

        public string DisplayBalance => Balance.ToString("C");

        private readonly int currentUserID = SessionManager.CurrentUserID;
        private readonly IUserRepository userRepo;
        private readonly ITransactionRepository transactionRepo;

        private WalletViewModel walletViewModel;
        private MovieViewModel shopViewModel;

        public IRelayCommand NavigateToShopCommand { get; }
        public IRelayCommand NavigateToWalletCommand { get; }
        public IRelayCommand NavigateToMarketplaceCommand { get; }
        public IRelayCommand NavigateToTicketsCommand { get; }
        public IRelayCommand NavigateToInventoryCommand { get; }

        public MainViewModel(IUserRepository userRepo, IActiveSalesRepository salesRepo, ITransactionRepository transactionRepo)
        {
            this.userRepo = userRepo;
            this.salesRepo = salesRepo;
            this.transactionRepo = transactionRepo;
            if (currentUserID > 0)
            {
                Balance = userRepo.GetBalance(currentUserID);
            }
            else
            {
                Balance = 0;
            }

            var currentSales = salesRepo.GetCurrentSales();
            var activeSale = currentSales.FirstOrDefault();

            DateTime expiry = activeSale?.EndTime ?? DateTime.Now;

            FlashSaleVM = new FlashSaleViewModel(expiry, () =>
            {
                System.Diagnostics.Debug.WriteLine("MainVM noticed the sale ended");
            });

            MovieShop.Services.SaleService.CurrentSale = this.FlashSaleVM;

            walletViewModel = new WalletViewModel(currentUserID, Balance, userRepo, transactionRepo);
            walletViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(WalletViewModel.Balance))
                {
                    Balance = walletViewModel.Balance;
                }
            };

            NavigateToShopCommand = new RelayCommand(NavigateToShop);
            NavigateToWalletCommand = new RelayCommand(NavigateToWallet);
            NavigateToMarketplaceCommand = new RelayCommand(NavigateToMarketplace);
            NavigateToTicketsCommand = new RelayCommand(NavigateToTickets);
            NavigateToInventoryCommand = new RelayCommand(NavigateToInventory);

            NavigateToShop();
        }

        public void RefreshBalanceFromDatabase()
        {
            if (currentUserID <= 0)
            {
                Balance = 0;
                walletViewModel.Balance = 0;
                return;
            }

            var b = userRepo.GetBalance(currentUserID);
            Balance = b;
            walletViewModel.Balance = b;
        }
        public void RefreshWallet()
        {
            RefreshBalanceFromDatabase();
            _ = walletViewModel.LoadTransactionsAsync();
        }

        private void NavigateToShop() => CurrentViewModel = "Shop";
        private void NavigateToWallet() => CurrentViewModel = walletViewModel;
        private void NavigateToMarketplace() => CurrentViewModel = "Marketplace";
        private void NavigateToInventory() => CurrentViewModel = "Inventory";
        private void NavigateToTickets() => CurrentViewModel = "Tickets";
        private void NavigateToSales() => CurrentViewModel = "SalesPage";
    }
}