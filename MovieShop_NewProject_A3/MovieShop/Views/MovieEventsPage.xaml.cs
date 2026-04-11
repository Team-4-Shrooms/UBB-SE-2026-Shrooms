using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.ViewModels;

namespace MovieShop.Views;

public sealed partial class MovieEventsPage : Page
{
    public MovieEventsViewModel ViewModel { get; } = App.Services.GetRequiredService<MovieEventsViewModel>();

    public MovieEventsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Initialize(e.Parameter as MovieEventsNavArgs);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            ViewModel.SearchQuery = textBox.Text;
        }
    }

    private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
        {
            ViewModel.DateFilter = item.Content as string ?? "All";
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
        }
    }

    private async void BuyTicket_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.DataContext is not MovieEventListItem item)
        {
            return;
        }

        if (!ViewModel.TryPurchase(item, out var error))
        {
            var errorDialog = new ContentDialog
            {
                Title = "Cannot complete purchase",
                Content = error,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot,
            };
            await errorDialog.ShowAsync();
            return;
        }

        var successDialog = new ContentDialog
        {
            Title = "Purchase successful",
            Content = $"Ticket for '{item.Event.Title}' purchased and added to your library.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        await successDialog.ShowAsync();

        if (XamlRoot?.Content is NavigationPage navPage)
        {
            navPage.ViewModel.RefreshWallet();
            navPage.ViewModel.CurrentViewModel = "Inventory";
        }
    }
}
