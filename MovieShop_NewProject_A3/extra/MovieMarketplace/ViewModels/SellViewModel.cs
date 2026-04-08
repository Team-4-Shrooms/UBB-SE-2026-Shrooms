using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MovieMarketplace.ViewModels
{
    public class SellViewModel : INotifyPropertyChanged
    {
        private string newItemTitle = string.Empty;
        private string newItemPrice = string.Empty;
        private string newItemDesc = string.Empty;

        public string NewItemTitle
        {
            get => newItemTitle;
            set { newItemTitle = value; OnPropertyChanged(); }
        }

        public string NewItemPrice
        {
            get => newItemPrice;
            set { newItemPrice = value; OnPropertyChanged(); }
        }

        public string NewItemDesc
        {
            get => newItemDesc;
            set { newItemDesc = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}