using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MovieShop.Models;
using MovieShop.Repositories;
using CommunityToolkit.Mvvm.Input;

namespace MovieShop.ViewModels
{
    public class WalletViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private int currentUserID;

        // --- Balance ---
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

        // --- TopUp Form Fields ---
        private string cardHolderName = string.Empty;
        public string CardHolderName
        {
            get => cardHolderName;
            set
            {
                cardHolderName = value;
                OnPropertyChanged(nameof(CardHolderName));
            }
        }

        private string cardNumber = string.Empty;
        public string CardNumber
        {
            get => cardNumber;
            set
            {
                cardNumber = value;
                OnPropertyChanged(nameof(CardNumber));
            }
        }

        private string expirationDate = string.Empty;
        public string ExpirationDate
        {
            get => expirationDate;
            set
            {
                expirationDate = value;
                OnPropertyChanged(nameof(ExpirationDate));
            }
        }

        private string cvv = string.Empty;
        public string CVV
        {
            get => cvv;
            set
            {
                cvv = value;
                OnPropertyChanged(nameof(CVV));
            }
        }

        // --- TopUpAmount as double for NumberBox binding ---
        private double topUpAmount;
        public double TopUpAmount
        {
            get => topUpAmount;
            set
            {
                topUpAmount = value;
                OnPropertyChanged(nameof(TopUpAmount));
            }
        }

        // --- Feedback Messages ---
        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private string successMessage = string.Empty;
        public string SuccessMessage
        {
            get => successMessage;
            set
            {
                successMessage = value;
                OnPropertyChanged(nameof(SuccessMessage));
            }
        }

        // --- Loading State ---
        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        // --- Transaction History ---
        private ObservableCollection<Transaction> transactions;
        public ObservableCollection<Transaction> Transactions
        {
            get => transactions;
            set
            {
                transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        // --- Commands ---
        public IRelayCommand TopUpCommand { get; }
        public IAsyncRelayCommand LoadTransactionsCommand { get; }

        // --- Repos ---
        private readonly ITransactionRepository transactionRepo;
        private readonly IUserRepository userRepo;

        // --- Constructor ---
        public WalletViewModel(int userID, decimal currentBalance, IUserRepository userRepo, ITransactionRepository transactionRepo)
        {
            this.userRepo = userRepo;
            this.transactionRepo = transactionRepo;
            currentUserID = userID;
            balance = currentBalance;
            transactions = new ObservableCollection<Transaction>();
            TopUpCommand = new RelayCommand(ExecuteTopUp);
            LoadTransactionsCommand = new AsyncRelayCommand(LoadTransactionsAsync);
        }

        // --- Load Transactions ---
        public async Task LoadTransactionsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var result = await Task.Run(() => transactionRepo.GetTransactionsByUserId(currentUserID));

                Transactions.Clear();
                foreach (var transaction in result)
                {
                    Transactions.Add(transaction);
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Failed to load transactions: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LogTopUpTransaction(decimal amount)
        {
            var transaction = new Transaction
            {
                BuyerID = new User { ID = currentUserID },
                Amount = amount,
                Type = "TopUp",
                Status = "Completed",
                Timestamp = System.DateTime.Now
            };

            Task.Run(() => transactionRepo.LogTransaction(transaction));

            Transactions.Insert(0, transaction);
        }

        private void SortTransactions()
        {
            var sorted = Transactions.OrderByDescending(transaction => transaction.Timestamp).ToList();
            Transactions.Clear();
            foreach (var transaction in sorted)
            {
                Transactions.Add(transaction);
            }
        }

        // --- TopUp Logic ---
        private void ExecuteTopUp()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (!ValidateCard())
            {
                return;
            }

            UpdateBalance((decimal)TopUpAmount);
            LogTopUpTransaction((decimal)TopUpAmount);

            SuccessMessage = $"Successfully added {TopUpAmount:C} to your wallet!";
            ClearForm();
        }

        // --- Validation ---
        private bool ValidateCard()
        {
            if (string.IsNullOrWhiteSpace(CardHolderName))
            {
                ErrorMessage = "Please enter the cardholder name.";
                return false;
            }

            foreach (char c in CardHolderName)
            {
                if (!char.IsLetter(c) && c != ' ')
                {
                    ErrorMessage = "Cardholder name can only contain letters and spaces.";
                    return false;
                }
            }

            var parts = CardHolderName.Trim().Split(' ');
            if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            {
                ErrorMessage = "Please enter both first and last name.";
                return false;
            }

            foreach (var part in parts)
            {
                if (part.Length < 2)
                {
                    ErrorMessage = "Each name must be at least 2 characters long.";
                    return false;
                }
            }

            if (CardNumber.Length != 16 || !long.TryParse(CardNumber, out _))
            {
                ErrorMessage = "Card number must be exactly 16 digits.";
                return false;
            }

            if (!System.DateTime.TryParseExact(ExpirationDate, "MM/yy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var expDate))
            {
                ErrorMessage = "Invalid expiration date. Use MM/YY format.";
                return false;
            }

            var lastDayOfMonth = new System.DateTime(expDate.Year, expDate.Month,
                System.DateTime.DaysInMonth(expDate.Year, expDate.Month));

            if (lastDayOfMonth < System.DateTime.Now)
            {
                ErrorMessage = "Your card has expired.";
                return false;
            }

            if (CVV.Length != 3 || !int.TryParse(CVV, out _))
            {
                ErrorMessage = "CVV must be exactly 3 digits.";
                return false;
            }

            if (TopUpAmount <= 0)
            {
                ErrorMessage = "Amount must be greater than 0.";
                return false;
            }

            return true;
        }

        // --- Helpers ---
        private void UpdateBalance(decimal amount)
        {
            Balance += amount;
            userRepo.UpdateBalance(currentUserID, Balance);
        }

        private void ClearForm()
        {
            CardHolderName = string.Empty;
            CardNumber = string.Empty;
            ExpirationDate = string.Empty;
            CVV = string.Empty;
            TopUpAmount = 0;
        }

        public void OnTransactionCompleted(decimal amount)
        {
            Balance += amount;
            userRepo.UpdateBalance(currentUserID, Balance);
            _ = LoadTransactionsAsync();
        }
    }
}