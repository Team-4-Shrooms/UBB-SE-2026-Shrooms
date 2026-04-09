using System;
using System.Collections.Generic;
using System.Linq;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class EventTicketService : IEventTicketService
    {
        private readonly IEventRepository eventRepo;
        private readonly IUserRepository userRepo;

        public EventTicketService(IEventRepository eventRepo, IUserRepository userRepo)
        {
            this.eventRepo = eventRepo;
            this.userRepo = userRepo;
        }

        public bool CanBuyTicket(int userId, MovieEvent movieEvent)
        {
            if (userId <= 0 || movieEvent == null)
            {
                return false;
            }

            var balance = userRepo.GetBalance(userId);
            SessionManager.CurrentUserBalance = balance;
            return balance >= movieEvent.TicketPrice;
        }

        // Moved from MovieEventsPage.xaml.cs:144-220 and BuyTicketPage.xaml.cs:84-143
        public void PurchaseTicket(int userId, MovieEvent movieEvent)
        {
            eventRepo.PurchaseTicket(userId, movieEvent.ID);
            SessionManager.CurrentUserBalance = userRepo.GetBalance(userId);
        }

        public List<MovieEvent> FilterEvents(List<MovieEvent> events, string searchQuery, string dateFilter)
        {
            var filtered = events.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                filtered = filtered.Where(movieEvent =>
                    (movieEvent.Title ?? string.Empty).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (movieEvent.Description ?? string.Empty).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (movieEvent.Location ?? string.Empty).IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (dateFilter == "Upcoming")
            {
                filtered = filtered.Where(movieEvent => movieEvent.Date >= DateTime.Now);
            }
            else if (dateFilter == "Past")
            {
                filtered = filtered.Where(movieEvent => movieEvent.Date < DateTime.Now);
            }

            return filtered.ToList();
        }
    }
}