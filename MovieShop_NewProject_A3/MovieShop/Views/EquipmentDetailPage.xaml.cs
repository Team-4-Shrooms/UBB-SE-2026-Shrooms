using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Models;
using MovieShop.ViewModels;

namespace MovieShop.Views
{
    public sealed partial class EquipmentDetailPage : Page
    {
        public EquipmentDetailViewModel ViewModel { get; } = App.Services.GetRequiredService<EquipmentDetailViewModel>();

        public EquipmentDetailPage(Equipment item)
        {
            this.InitializeComponent();
            ViewModel.Initialize(item);
            LoadImage();
        }

        private void LoadImage()
        {
            if (ViewModel.Equipment == null || string.IsNullOrEmpty(ViewModel.ImageUrl))
            {
                return;
            }

            try
            {
                ItemImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(ViewModel.ImageUrl));
            }
            catch (UriFormatException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EquipmentDetailPage] Invalid image URL '{ViewModel.ImageUrl}': {ex.Message}");
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Visible;

        private void CancelShipping_Click(object sender, RoutedEventArgs e) => ShippingModal.Visibility = Visibility.Collapsed;

        private async void ConfirmShipping_Click(object sender, RoutedEventArgs e)
        {
            ModalErrorText.Visibility = Visibility.Collapsed;

            var validationError = ViewModel.ValidateShipping(
                ModalNameInput.Text,
                ModalAddressInput.Text,
                ModalPhoneInput.Text);

            if (validationError != null)
            {
                ModalErrorText.Text = validationError;
                ModalErrorText.Visibility = Visibility.Visible;
                return;
            }

            if (!ViewModel.TryPurchase(ModalAddressInput.Text, out var error))
            {
                ModalErrorText.Text = error;
                ModalErrorText.Visibility = Visibility.Visible;
                return;
            }

            if (App.CurrentWindow?.Content is NavigationPage navPage)
            {
                navPage.ViewModel.RefreshWallet();
            }

            ShippingModal.Visibility = Visibility.Collapsed;

            var dialog = new ContentDialog
            {
                Title = "Purchase successful",
                Content = $"\"{ViewModel.Title}\" has been purchased and added to your inventory.",
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

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentArea)
            {
                contentArea.Content = new MarketplacePage();
            }
        }
    }
}
