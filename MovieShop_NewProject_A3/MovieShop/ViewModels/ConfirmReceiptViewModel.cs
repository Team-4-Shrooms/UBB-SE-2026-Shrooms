using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.ViewModels
{
    public class ConfirmReceiptViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly IUserRepository userRepo = App.Services.GetRequiredService<IUserRepository>();
        private int currentUserID;

        private Transaction transaction;
        public Transaction Transaction
        {
            get => transaction;
            set
            {
                transaction = value;
                OnPropertyChanged(nameof(Transaction));
            }
        }

        private decimal sellerBalance;
        public decimal SellerBalance
        {
            get => sellerBalance;
            set
            {
                sellerBalance = value;
                OnPropertyChanged(nameof(SellerBalance));
            }
        }

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

        private bool isConfirmButtonVisible;
        public bool IsConfirmButtonVisible
        {
            get => isConfirmButtonVisible;
            set
            {
                isConfirmButtonVisible = value;
                OnPropertyChanged(nameof(IsConfirmButtonVisible));
            }
        }

        private bool isSuccessVisible;
        public bool IsSuccessVisible
        {
            get => isSuccessVisible;
            set
            {
                isSuccessVisible = value;
                OnPropertyChanged(nameof(IsSuccessVisible));
            }
        }

        public IRelayCommand ConfirmReceiptCommand { get; }
        public IRelayCommand DismissSuccessCommand { get; }

        public ConfirmReceiptViewModel(int userID, Transaction transaction, decimal sellerBalance)
        {
            currentUserID = userID;
            this.transaction = transaction;
            this.sellerBalance = sellerBalance;

            IsConfirmButtonVisible = transaction.Status == "Pending";

            ConfirmReceiptCommand = new RelayCommand(ConfirmReceipt);
            DismissSuccessCommand = new RelayCommand(DismissSuccess);
        }

        private void ConfirmReceipt()
        {
            ErrorMessage = string.Empty;

            if (Transaction == null)
            {
                ErrorMessage = "No transaction found.";
                return;
            }

            if (Transaction.Status != "Pending")
            {
                ErrorMessage = "This transaction has already been completed.";
                return;
            }

            if (Transaction.BuyerID.ID != currentUserID)
            {
                ErrorMessage = "Only the buyer can confirm receipt.";
                return;
            }

            ReleaseEscrowToSeller();
            UpdateTransactionStatus("Completed");

            IsConfirmButtonVisible = false;
            IsSuccessVisible = true;
            SuccessMessage = "Receipt confirmed! The seller has been paid.";
        }

        private void ReleaseEscrowToSeller()
        {
            if (Transaction.SellerID == null)
            {
                return;
            }

            var currentBalance = userRepo.GetBalance(Transaction.SellerID.ID);
            var newBalance = currentBalance + Transaction.Amount;
            userRepo.UpdateBalance(Transaction.SellerID.ID, newBalance);
            SellerBalance = newBalance;
        }

        private void UpdateTransactionStatus(string newStatus)
        {
            Transaction.Status = newStatus;
            OnPropertyChanged(nameof(Transaction));
            System.Diagnostics.Debug.WriteLine(
                $"[ConfirmReceiptViewModel] Status set to '{newStatus}' locally. " +
                "Add UpdateStatus to ITransactionRepository to persist to DB.");
        }

        private void DismissSuccess()
        {
            IsSuccessVisible = false;
            SuccessMessage = string.Empty;
        }
    }
}