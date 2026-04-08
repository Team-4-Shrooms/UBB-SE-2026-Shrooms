using System;
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
    }
}