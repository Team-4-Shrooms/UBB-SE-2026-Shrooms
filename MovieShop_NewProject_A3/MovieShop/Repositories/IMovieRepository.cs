using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public interface IMovieRepository
    {
        List<Movie> GetAllMovies();

        Movie? GetMovieById(int movieId);

        bool UserOwnsMovie(int userId, int movieId);

        void PurchaseMovie(int userId, int movieId, decimal finalPrice);
    }
}
