using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class MarketplaceViewModel : INotifyPropertyChanged
    {
        private readonly IMarketplaceService marketplaceService;

        private List<Equipment> allOriginalItems = new List<Equipment>();

        public ObservableCollection<Equipment> AvailableItems { get; set; } = new ObservableCollection<Equipment>();

        public decimal UserBalance => SessionManager.CurrentUserBalance;

        public MarketplaceViewModel(IMarketplaceService marketplaceService)
        {
            this.marketplaceService = marketplaceService;
            LoadData();
        }

        public void LoadData()
        {
            allOriginalItems = marketplaceService.GetAvailableEquipment();
            UpdateDisplayList(allOriginalItems);
        }

        public void FilterByCategory(string? category)
        {
            var filtered = marketplaceService.FilterByCategory(allOriginalItems, category);
            UpdateDisplayList(filtered);
        }

        private void UpdateDisplayList(List<Equipment> items)
        {
            AvailableItems.Clear();
            foreach (var item in items)
            {
                if (item != null)
                {
                    AvailableItems.Add(item);
                }
            }

            OnPropertyChanged(nameof(AvailableItems));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string StatusMessage => SessionManager.CurrentUserID == 0
            ? "Please log in to purchase equipment."
            : string.Empty;
    }
}
