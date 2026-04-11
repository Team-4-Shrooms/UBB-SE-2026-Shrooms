using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Services
{
    public interface IInventoryService
    {
        List<Movie> GetOwnedMovies(int userId);

        List<MovieEvent> GetOwnedTickets(int userId);

        List<Equipment> GetOwnedEquipment(int userId);

        IEnumerable<Movie> RemoveMovie(int userId, int movieId);

        IEnumerable<MovieEvent> RemoveTicket(int userId, int eventId);
    }
}