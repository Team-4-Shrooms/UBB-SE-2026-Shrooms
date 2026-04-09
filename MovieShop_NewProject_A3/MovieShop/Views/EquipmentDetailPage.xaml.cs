using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

namespace MovieShop.Views
{
    public sealed partial class EquipmentDetailPage : Page
    {
        private readonly IEquipmentPurchaseService purchaseService = App.Services.GetRequiredService<IEquipmentPurchaseService>();
        private Equipment selectedItem;

        public EquipmentDetailPage(Equipment item)
        {
            this.InitializeComponent();
            selectedItem = item;
            PopulateUI();
        }

        private void PopulateUI()
        {
            if (selectedItem == null)
            {
                return;
            }

            TitleLabel.Text = selectedItem.Title;
            DescriptionLabel.Text = selectedItem.Description ?? "No description available.";
            CategoryLabel.Text = selectedItem.Category;
            ConditionLabel.Text = selectedItem.Condition;
            PriceLabel.Text = $"Price: ${selectedItem.Price:F2}";

            if (!string.IsNullOrEmpty(selectedItem.ImageUrl))
            {
                try
                {
                    ItemImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(selectedItem.ImageUrl));
                }
                catch (UriFormatException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[EquipmentDetailPage] Invalid image URL '{selectedItem.ImageUrl}': {ex.Message}");
                }
            }

            var canAfford = purchaseService.CanAfford(SessionManager.CurrentUserID, selectedItem.Price);
            ConfirmBuyButton.IsEnabled = canAfford;
            ErrorText.Visibility = canAfford ? Visibility.Collapsed : Visibility.Visible;
            if (!canAfford)
            {
                ErrorText.Text = $"Insufficient funds. Balance: {SessionManager.CurrentUserBalance:C} — Price: {selectedItem.Price:C}";
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Visible;
        private void CancelShipping_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Collapsed;

        private async void ConfirmShipping_Click(object sender, RoutedEventArgs e)
        {
            ModalErrorText.Visibility = Visibility.Collapsed;

            string error = purchaseService.ValidateShippingDetails(
                ModalNameInput.Text,
                ModalAddressInput.Text,
                ModalPhoneInput.Text);

            if (!string.IsNullOrEmpty(error))
            {
                ModalErrorText.Text = error;
                ModalErrorText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                purchaseService.PurchaseEquipment(
                    selectedItem.ID,
                    SessionManager.CurrentUserID,
                    selectedItem.Price,
                    ModalAddressInput.Text);

                if (App.CurrentWindow?.Content is NavigationPage navPage)
                {
                    navPage.ViewModel.RefreshWallet();
                }

                ShippingModal.Visibility = Visibility.Collapsed;

                var dialog = new ContentDialog
                {
                    Title = "Purchase successful",
                    Content = $"\"{selectedItem.Title}\" has been purchased and added to your inventory.",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = XamlRoot
                };
                await dialog.ShowAsync();

                if (this.Parent is ContentControl contentArea)
                {
                    contentArea.Content = new MarketplacePage();
                }
            }
            catch (InvalidOperationException ex)
            {
                ModalErrorText.Text = ex.Message;
                ModalErrorText.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EquipmentDetailPage] Unexpected purchase error: {ex}");
                ModalErrorText.Text = "Transaction failed: " + ex.Message;
                ModalErrorText.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
            {
                contentArea.Content = new MarketplacePage();
            }
        }
    }
}