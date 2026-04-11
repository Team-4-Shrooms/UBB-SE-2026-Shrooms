using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class SellEquipmentViewModel : INotifyPropertyChanged
    {
        private readonly IMarketplaceService marketplaceService;

        public SellEquipmentViewModel(IMarketplaceService marketplaceService)
        {
            this.marketplaceService = marketplaceService;
        }

        private string newItemTitle = string.Empty;
        private string newItemDesc = string.Empty;
        private string priceInput = string.Empty;
        private decimal validatedPrice;
        private string priceErrorMessage = string.Empty;
        private bool canPost;

        public string NewItemTitle
        {
            get => newItemTitle;
            set
            {
                newItemTitle = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string NewItemDesc
        {
            get => newItemDesc;
            set
            {
                newItemDesc = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string PriceInput
        {
            get => priceInput;
            set
            {
                priceInput = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string PriceErrorMessage
        {
            get => priceErrorMessage;
            set
            {
                priceErrorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool CanPost
        {
            get => canPost;
            set
            {
                canPost = value;
                OnPropertyChanged();
            }
        }

        public decimal ValidatedPrice => validatedPrice;

        public void SubmitListing(string? category, string? condition, string imageUrl)
        {
            var newItem = new Equipment
            {
                SellerID = SessionManager.CurrentUserID,
                Title = NewItemTitle,
                Description = NewItemDesc,
                Price = ValidatedPrice,
                Category = category,
                Condition = condition,
                ImageUrl = imageUrl,
                Status = EquipmentStatus.Available
            };

            marketplaceService.ListItem(newItem);
        }

        private void ValidateForm()
        {
            bool isPriceValid = decimal.TryParse(priceInput, out decimal result);
            bool isTitleValid = !string.IsNullOrWhiteSpace(newItemTitle);

            if (!isPriceValid && !string.IsNullOrEmpty(priceInput))
            {
                PriceErrorMessage = "Please enter a valid numeric price!";
                CanPost = false;
                return;
            }

            if (isPriceValid && result <= 0)
            {
                PriceErrorMessage = "Price must be greater than 0!";
                CanPost = false;
                return;
            }

            if (isPriceValid && isTitleValid)
            {
                validatedPrice = result;
                PriceErrorMessage = string.Empty;
                CanPost = true;
            }
            else
            {
                CanPost = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}