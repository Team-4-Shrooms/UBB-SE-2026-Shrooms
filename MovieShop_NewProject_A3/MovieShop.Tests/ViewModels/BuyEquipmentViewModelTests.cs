using MovieShop.Models;
using MovieShop.ViewModels;
using System.ComponentModel;
using Xunit;

namespace MovieShop.Tests.ViewModels
{
    public class BuyEquipmentViewModelTests
    {
        public BuyEquipmentViewModelTests()
        {
            // Reset SessionManager state
            SessionManager.CurrentUserID = 0;
            SessionManager.CurrentUserBalance = 0;
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
            SessionManager.CurrentUserID = 0;
            var viewModel = new BuyEquipmentViewModel { SelectedItem = new Equipment { Price = 10 } };
            Assert.False(viewModel.CanPurchase);
        }

        [Fact]
        public void CanPurchase_InsufficientBalance_ReturnsFalse()
        {
            SessionManager.CurrentUserID = 1;
            SessionManager.CurrentUserBalance = 5m;
            var viewModel = new BuyEquipmentViewModel { SelectedItem = new Equipment { Price = 10m } };
            Assert.False(viewModel.CanPurchase);
        }

        [Fact]
        public void CanPurchase_SufficientBalanceAndLoggedIn_ReturnsTrue()
        {
            SessionManager.CurrentUserID = 1;
            SessionManager.CurrentUserBalance = 15m;
            var viewModel = new BuyEquipmentViewModel { SelectedItem = new Equipment { Price = 10m } };
            Assert.True(viewModel.CanPurchase);
        }

        [Fact]
        public void SelectedItem_Set_RaisesPropertyChanges()
        {
            var viewModel = new BuyEquipmentViewModel();
            var propertyChanges = new System.Collections.Generic.List<string>();
            viewModel.PropertyChanged += (sender, e) => propertyChanges.Add(e.PropertyName);

            viewModel.SelectedItem = new Equipment();

            Assert.Contains("SelectedItem", propertyChanges);
            Assert.Contains("CanPurchase", propertyChanges);
        }
    }
}
