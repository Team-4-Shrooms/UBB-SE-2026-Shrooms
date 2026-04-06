using MovieShop.Models;
using MovieShop.Repositories;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MovieShop.ViewModels
{
    public class MarketplaceViewModel : INotifyPropertyChanged
    {
        private readonly IEquipmentRepository _repository;

        private List<Equipment> _allOriginalItems = new List<Equipment>();

        public ObservableCollection<Equipment> AvailableItems { get; set; } = new ObservableCollection<Equipment>();

        public decimal UserBalance
        {
            get
            {
                return SessionManager.CurrentUserBalance;
            }
        }

        public MarketplaceViewModel(IEquipmentRepository equipmentRepo)
        {
            _repository = equipmentRepo;
            LoadData();
        }

        public void LoadData()
        {
            var data = _repository.FetchAvailableEquipment() ?? new List<Equipment>();
            _allOriginalItems = data;

            UpdateDisplayList(_allOriginalItems);
        }

        public void FilterByCategory(string? category)
        {
            var filtered = string.IsNullOrEmpty(category) || category == "All"
                ? _allOriginalItems
                : _allOriginalItems.Where(item => item.Category == category).ToList();

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