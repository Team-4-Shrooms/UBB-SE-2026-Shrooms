using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class MovieEventsViewModel : INotifyPropertyChanged
    {
        private readonly IEventTicketService ticketService;
        private List<MovieEvent> allEvents = new ();
        private string title = "Events";
        private string searchQuery = string.Empty;
        private string dateFilter = "All";

        public MovieEventsViewModel(IEventTicketService ticketService)
        {
            this.ticketService = ticketService;
            Events = new ObservableCollection<MovieEventListItem>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MovieEventListItem> Events { get; }

        public string Title
        {
            get => title;
            private set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                searchQuery = value ?? string.Empty;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public string DateFilter
        {
            get => dateFilter;
            set
            {
                dateFilter = string.IsNullOrWhiteSpace(value) ? "All" : value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        public void Initialize(MovieEventsNavArgs? args)
        {
            var movie = args?.Movie;

            if (movie == null)
            {
                Title = "Events";
                allEvents = ticketService.GetAllEvents();
            }
            else
            {
                Title = $"Events - {movie.Title}";
                allEvents = ticketService.GetEventsForMovie(movie.ID);
            }

            ApplyFilters();
        }

        public bool TryPurchase(MovieEventListItem item, out string error)
        {
            error = string.Empty;
            if (item == null)
            {
                error = "No event selected.";
                return false;
            }

            try
            {
                ticketService.PurchaseTicket(SessionManager.CurrentUserID, item.Event);
                RefreshBuyStates();
                return true;
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private void ApplyFilters()
        {
            var filtered = ticketService.FilterEvents(allEvents, searchQuery, dateFilter);

            Events.Clear();
            foreach (var movieEvent in filtered)
            {
                var canBuy = ticketService.CanBuyTicket(SessionManager.CurrentUserID, movieEvent);
                Events.Add(new MovieEventListItem(movieEvent, canBuy));
            }
        }

        private void RefreshBuyStates()
        {
            foreach (var item in Events)
            {
                item.CanBuy = ticketService.CanBuyTicket(SessionManager.CurrentUserID, item.Event);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
