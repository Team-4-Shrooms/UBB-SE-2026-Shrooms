using MovieShop.Models;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public interface IReviewRepository
    {
        List<MovieReview> GetReviewsForMovie(int movieId);

        void AddReview(int movieId, int userId, int starRating, string? comment);
    }
}
