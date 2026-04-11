using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class EquipmentDetailViewModel : INotifyPropertyChanged
    {
        private readonly IEquipmentPurchaseService purchaseService;

        private Equipment? equipment;
        private bool canAfford;
        private string errorText = string.Empty;

        public EquipmentDetailViewModel(IEquipmentPurchaseService purchaseService)
        {
            this.purchaseService = purchaseService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Equipment? Equipment
        {
            get => equipment;
            private set
            {
                equipment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(Condition));
                OnPropertyChanged(nameof(PriceText));
                OnPropertyChanged(nameof(ImageUrl));
            }
        }

        public string Title => equipment?.Title ?? string.Empty;

        public string Description => string.IsNullOrEmpty(equipment?.Description)
            ? "No description available."
            : equipment!.Description!;

        public string Category => equipment?.Category ?? string.Empty;

        public string Condition => equipment?.Condition ?? string.Empty;

        public string PriceText => equipment == null ? string.Empty : $"Price: ${equipment.Price:F2}";

        public string? ImageUrl => equipment?.ImageUrl;

        public bool CanAfford
        {
            get => canAfford;
            private set
            {
                canAfford = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ErrorVisibility));
            }
        }

        public string ErrorText
        {
            get => errorText;
            private set
            {
                errorText = value;
                OnPropertyChanged();
            }
        }

        public Visibility ErrorVisibility => CanAfford ? Visibility.Collapsed : Visibility.Visible;

        public void Initialize(Equipment? item)
        {
            Equipment = item;

            if (item == null)
            {
                CanAfford = false;
                ErrorText = string.Empty;
                return;
            }

            CanAfford = purchaseService.CanAfford(SessionManager.CurrentUserID, item.Price);
            ErrorText = CanAfford
                ? string.Empty
                : $"Insufficient funds. Balance: {SessionManager.CurrentUserBalance:C} — Price: {item.Price:C}";
        }

        public string? ValidateShipping(string name, string address, string phone)
        {
            var error = purchaseService.ValidateShippingDetails(name, address, phone);
            return string.IsNullOrEmpty(error) ? null : error;
        }

        public bool TryPurchase(string address, out string error)
        {
            error = string.Empty;

            if (equipment == null)
            {
                error = "No equipment selected.";
                return false;
            }

            try
            {
                purchaseService.PurchaseEquipment(
                    equipment.ID,
                    SessionManager.CurrentUserID,
                    equipment.Price,
                    address);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                error = "Transaction failed: " + ex.Message;
                return false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
