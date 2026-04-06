using MovieShop.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MovieShop.ViewModels
{
    public class BuyEquipmentViewModel : INotifyPropertyChanged
    {
        private Equipment? _selectedItem;
        public Equipment? SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanPurchase));
            }
        }

        public bool CanPurchase
        {
            get
            {
                if (SelectedItem == null) return false;
                bool isLoggedIn = SessionManager.CurrentUserID > 0;

                decimal userBalance = SessionManager.CurrentUserBalance;

                return isLoggedIn && userBalance >= SelectedItem.Price;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}