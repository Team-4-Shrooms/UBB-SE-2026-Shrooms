using MovieShop.Models;
using System.Collections.Generic;

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
