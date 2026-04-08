using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public interface IReviewRepository
    {
        List<MovieReview> GetReviewsForMovie(int movieId);

        void AddReview(int movieId, int userId, int starRating, string? comment);
    }
}
