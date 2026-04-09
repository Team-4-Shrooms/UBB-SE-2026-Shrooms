using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public interface IEventRepository
    {
        List<MovieEvent> GetEventsForMovie(int movieId);

        List<MovieEvent> GetAllEvents();

        MovieEvent? GetEventById(int eventId);

        void PurchaseTicket(int userId, int eventId);
    }
}
