using System;
using System.Collections.Generic;
using System.Linq;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class MovieReviewService : IMovieReviewService
    {
        private const int MinStarRating = 1;
        private const int MaxStarRating = 10;

        private readonly IReviewRepository reviewRepo;

        public MovieReviewService(IReviewRepository reviewRepo)
        {
            this.reviewRepo = reviewRepo;
        }

        public List<MovieReview> GetReviewsForMovie(int movieId)
        {
            return reviewRepo.GetReviewsForMovie(movieId);
        }

        public int GetReviewCount(int movieId)
        {
            return reviewRepo.GetReviewCount(movieId);
        }

        public Dictionary<int, int> GetReviewCounts(IEnumerable<int> movieIds)
        {
            return reviewRepo.GetReviewCounts(movieIds);
        }

        public string BuildStarDistributionTooltip(int movieId)
        {
            var counts = reviewRepo.GetStarRatingBuckets(movieId);

            int total = counts.Skip(1).Sum();
            if (total == 0)
            {
                return "No reviews yet.";
            }

            var lines = new List<string> { "Rating distribution:" };
            for (int i = MaxStarRating; i >= MinStarRating; i--)
            {
                lines.Add($"{i}: {counts[i]}");
            }

            return string.Join("\n", lines);
        }

        public void AddReview(int movieId, int userId, int rating, string? comment)
        {
            if (userId <= 0)
            {
                throw new InvalidOperationException("You must be logged in to add a review.");
            }

            if (rating < MinStarRating || rating > MaxStarRating)
            {
                throw new InvalidOperationException($"Rating must be between {MinStarRating} and {MaxStarRating}.");
            }

            reviewRepo.AddReview(movieId, userId, rating, comment);
        }
    }
}
