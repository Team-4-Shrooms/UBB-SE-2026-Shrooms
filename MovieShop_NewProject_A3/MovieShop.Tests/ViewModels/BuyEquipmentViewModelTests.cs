using MovieShop.Models;
using MovieShop.ViewModels;

namespace MovieShop.Tests.ViewModels
{
    public class BuyEquipmentViewModelTests
    {
        private const int NotLoggedInUserId = 0;
        private const int ValidUserId = 1;
        private const decimal ZeroBalance = 0m;
        private const decimal InsufficientBalance = 5m;
        private const decimal SufficientBalance = 15m;
        private const decimal ItemPrice = 10m;

        public BuyEquipmentViewModelTests()
        {
            SessionManager.CurrentUserID = NotLoggedInUserId;
            SessionManager.CurrentUserBalance = ZeroBalance;
        }

        [Fact]
        public void CanPurchase_NoSelectedItem_ReturnsFalse()
        {
            var viewModel = new BuyEquipmentViewModel { SelectedItem = null };
            Assert.False(viewModel.CanPurchase);
        }

        [Fact]
        public void CanPurchase_NotLoggedIn_ReturnsFalse()
        {
            SessionManager.CurrentUserID = NotLoggedInUserId;
            var viewModel = new BuyEquipmentViewModel { SelectedItem = new Equipment { Price = ItemPrice } };
            Assert.False(viewModel.CanPurchase);
        }

        [Fact]
        public void CanPurchase_InsufficientBalance_ReturnsFalse()
        {
            SessionManager.CurrentUserID = ValidUserId;
            SessionManager.CurrentUserBalance = InsufficientBalance;
            var viewModel = new BuyEquipmentViewModel { SelectedItem = new Equipment { Price = ItemPrice } };
            Assert.False(viewModel.CanPurchase);
        }

        [Fact]
        public void CanPurchase_SufficientBalanceAndLoggedIn_ReturnsTrue()
        {
            SessionManager.CurrentUserID = ValidUserId;
            SessionManager.CurrentUserBalance = SufficientBalance;
            var viewModel = new BuyEquipmentViewModel { SelectedItem = new Equipment { Price = ItemPrice } };
            Assert.True(viewModel.CanPurchase);
        }

        [Fact]
        public void SelectedItem_Set_RaisesSelectedItemPropertyChange()
        {
            var viewModel = new BuyEquipmentViewModel();
            var propertyChanges = new List<string>();
            viewModel.PropertyChanged += (sender, e) => propertyChanges.Add(e.PropertyName);

            viewModel.SelectedItem = new Equipment();

            Assert.Contains("SelectedItem", propertyChanges);
        }

        [Fact]
        public void SelectedItem_Set_RaisesCanPurchasePropertyChange()
        {
            var viewModel = new BuyEquipmentViewModel();
            var propertyChanges = new List<string>();
            viewModel.PropertyChanged += (sender, e) => propertyChanges.Add(e.PropertyName);

            viewModel.SelectedItem = new Equipment();

            Assert.Contains("CanPurchase", propertyChanges);
        }
    }
}
