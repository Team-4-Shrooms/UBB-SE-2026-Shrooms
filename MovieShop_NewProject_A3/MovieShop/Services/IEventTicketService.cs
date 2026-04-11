using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Services
{
    public interface IEventTicketService
    {
        bool CanBuyTicket(int userId, MovieEvent movieEvent);

        void PurchaseTicket(int userId, MovieEvent movieEvent);

        List<MovieEvent> FilterEvents(List<MovieEvent> events, string searchQuery, string dateFilter);

        MovieEvent? GetEventById(int eventId);

        List<MovieEvent> GetAllEvents();

        List<MovieEvent> GetEventsForMovie(int movieId);
    }
}