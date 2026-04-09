using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

namespace MovieShop.Views;

public sealed partial class MovieEventsPage : Page
{
    private Movie? movie;
    private List<MovieEvent>? allEvents;
    private readonly IEventRepository eventRepo = App.Services.GetRequiredService<IEventRepository>();
    private readonly IUserRepository userRepo = App.Services.GetRequiredService<IUserRepository>();
    private readonly IEventTicketService ticketService = App.Services.GetRequiredService<IEventTicketService>();

    private static object? FindDescendantByName(DependencyObject parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is FrameworkElement frameworkElement && frameworkElement.Name == name)
            {
                return frameworkElement;
            }

            var found = FindDescendantByName(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    public MovieEventsPage()
    {
        InitializeComponent();
        this.Loaded += MovieEventsPage_Loaded;
    }

    private void MovieEventsPage_Loaded(object? sender, RoutedEventArgs e)
    {
        if (EventsList.ItemsSource == null)
        {
            TitleBlock.Text = "Events";
            allEvents = eventRepo.GetAllEvents();
            EventsList.ItemsSource = allEvents;
            UpdateBuyButtons();
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

    private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

    private void ApplyFilters()
    {
        if (allEvents == null)
        {
            return;
        }

        var searchQuery = SearchBox?.Text?.Trim() ?? string.Empty;
        var dateFilter = (FilterCombo?.SelectedItem as ComboBoxItem)?.Content as string ?? "All";

        EventsList.ItemsSource = ticketService.FilterEvents(allEvents, searchQuery, dateFilter);
        UpdateBuyButtons();
    }

    private void UpdateBuyButtons()
    {
        var userId = SessionManager.CurrentUserID;

        foreach (var item in EventsList.Items)
        {
            var lvi = EventsList.ContainerFromItem(item) as ListViewItem;
            if (lvi == null)
            {
                continue;
            }

            var btn = FindDescendantByName(lvi, "BuyTicketButton") as Button;
            if (btn == null)
            {
                continue;
            }

            var movieEvent = item as MovieEvent;
            var canBuy = movieEvent != null && ticketService.CanBuyTicket(userId, movieEvent);
            btn.IsEnabled = canBuy;
            btn.Opacity = canBuy ? UIConstants.EnabledButtonOpacity : UIConstants.DisabledButtonOpacity;
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is not MovieEventsNavArgs args)
        {
            return;
        }

        movie = args.Movie;
        if (movie == null)
        {
            TitleBlock.Text = "Events";
            allEvents = eventRepo.GetAllEvents();
            EventsList.ItemsSource = allEvents;
            return;
        }

        TitleBlock.Text = $"Events - {movie.Title}";
        allEvents = eventRepo.GetEventsForMovie(movie.ID);
        EventsList.ItemsSource = allEvents;
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
        if (sender is not FrameworkElement element || element.DataContext is not MovieEvent selectedEvent)
        {
            return;
        }

        if (!SessionManager.IsLoggedIn)
        {
            var dlg = new ContentDialog
            {
                Title = "Sign in",
                Content = "Please sign in to continue.",
                PrimaryButtonText = "Sign in",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };

            if (await dlg.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            SessionManager.CurrentUserID = 1;
            try
            {
                SessionManager.CurrentUserBalance = userRepo.GetBalance(1);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MovieEventsPage] Failed to fetch balance after login: {ex.Message}");
                SessionManager.CurrentUserBalance = 0m;
            }

            UpdateBuyButtons();
            return; // let the user click Buy again after signing in
        }

        try
        {
            await System.Threading.Tasks.Task.Run(() =>
                ticketService.PurchaseTicket(SessionManager.CurrentUserID, selectedEvent));

            UpdateBuyButtons();

            var dialog = new ContentDialog
            {
                Title = "Purchase successful",
                Content = $"Ticket for '{selectedEvent.Title}' purchased and added to your library.",
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await dialog.ShowAsync();

            if (this.XamlRoot?.Content is NavigationPage navPage)
            {
                navPage.ViewModel.RefreshWallet();
                navPage.ViewModel.CurrentViewModel = "Inventory";
            }
        }
        catch (InvalidOperationException ex)
        {
            var err = new ContentDialog
            {
                Title = "Cannot complete purchase",
                Content = ex.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await err.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MovieEventsPage] Unexpected purchase error: {ex}");
            var err = new ContentDialog
            {
                Title = "Error",
                Content = "An unexpected error occurred: " + ex.Message,
                CloseButtonText = "OK",
                XamlRoot = XamlRoot
            };
            await err.ShowAsync();
        }
    }
}