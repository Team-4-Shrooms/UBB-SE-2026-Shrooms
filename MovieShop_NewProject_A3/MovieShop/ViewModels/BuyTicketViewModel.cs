using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MovieShop.Models;
using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class BuyTicketViewModel : INotifyPropertyChanged
    {
        private readonly IEventTicketService ticketService;
        private MovieEvent? selectedEvent;
        private bool canConfirm;
        private string statusMessage = string.Empty;
        private bool hasStatusMessage;

        public BuyTicketViewModel(IEventTicketService ticketService)
        {
            this.ticketService = ticketService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MovieEvent? SelectedEvent
        {
            get => selectedEvent;
            private set
            {
                selectedEvent = value;
                OnPropertyChanged();
            }
        }

        public bool CanConfirm
        {
            get => canConfirm;
            private set
            {
                canConfirm = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            private set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }

        public bool HasStatusMessage
        {
            get => hasStatusMessage;
            private set
            {
                hasStatusMessage = value;
                OnPropertyChanged();
            }
        }

        public void Initialize(object? navigationParameter)
        {
            SelectedEvent = navigationParameter switch
            {
                int id => ticketService.GetEventById(id),
                MovieEvent movieEvent => movieEvent,
                _ => null,
            };

            RefreshButtonState();
        }

        public bool TryPurchase(out string error)
        {
            error = string.Empty;

            if (SelectedEvent == null)
            {
                error = "No event selected.";
                return false;
            }

            try
            {
                ticketService.PurchaseTicket(SessionManager.CurrentUserID, SelectedEvent);
                RefreshButtonState();
                return true;
            }
            catch (InvalidOperationException ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private void RefreshButtonState()
        {
            if (SelectedEvent == null)
            {
                CanConfirm = false;
                SetStatusMessage(string.Empty);
                return;
            }

            var canBuy = ticketService.CanBuyTicket(SessionManager.CurrentUserID, SelectedEvent);
            CanConfirm = canBuy;
            SetStatusMessage(canBuy
                ? string.Empty
                : $"Insufficient funds. Balance: {SessionManager.CurrentUserBalance:C} — Price: {SelectedEvent.TicketPrice:C}");
        }

        private void SetStatusMessage(string message)
        {
            StatusMessage = message;
            HasStatusMessage = !string.IsNullOrEmpty(message);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
