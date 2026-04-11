using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public sealed class ReviewRepo : IReviewRepository
    {
        private const int StarRatingBucketCount = 11;
        private const int MinStarRating = 1;
        private const int MaxStarRating = 10;

        private readonly IDatabaseSingleton db;

        public ReviewRepo()
            : this(DatabaseSingleton.Instance)
        {
        }

        public ReviewRepo(IDatabaseSingleton db)
        {
            this.db = db;
        }

        public List<MovieReview> GetReviewsForMovie(int movieId)
        {
            var list = new List<MovieReview>();
            const string query = @"SELECT r.ID, r.MovieID, r.UserID, u.Username, r.StarRating, r.Comment, r.CreatedAt
                                   FROM Reviews r
                                   JOIN Users u ON u.ID = r.UserID
                                   WHERE r.MovieID = @mid
                                   ORDER BY r.CreatedAt DESC, r.ID DESC";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);
                cmd.Parameters.AddWithValue("@mid", movieId);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MovieReview
                    {
                        ID = reader.GetInt32(0),
                        MovieID = reader.GetInt32(1),
                        UserID = reader.GetInt32(2),
                        Username = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                        StarRating = reader.GetInt32(4),
                        Comment = reader.IsDBNull(5) ? null : reader.GetString(5),
                        CreatedAt = reader.GetDateTime(6)
                    });
                }
            }
            finally
            {
                db.CloseConnection();
            }

            return list;
        }

        public void AddReview(int movieId, int userId, int starRating, string? comment)
        {
            const string query = @"INSERT INTO Reviews (MovieID, UserID, StarRating, Comment) VALUES (@mid, @uid, @star, @comment)";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);
                cmd.Parameters.AddWithValue("@mid", movieId);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@star", starRating);
                cmd.Parameters.AddWithValue("@comment", string.IsNullOrWhiteSpace(comment) ? (object)DBNull.Value : comment);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                db.CloseConnection();
            }
        }

        public int GetReviewCount(int movieId)
        {
            const string query = @"SELECT COUNT(*) FROM Reviews WHERE MovieID = @mid";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);
                cmd.Parameters.AddWithValue("@mid", movieId);
                var result = cmd.ExecuteScalar();
                return result is int count ? count : Convert.ToInt32(result);
            }
            finally
            {
                db.CloseConnection();
            }
        }

        public Dictionary<int, int> GetReviewCounts(IEnumerable<int> movieIds)
        {
            var result = new Dictionary<int, int>();

            var ids = movieIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return result;
            }

            var paramNames = ids.Select((_, index) => $"@id{index}").ToArray();
            var inClause = string.Join(",", paramNames);

            string query = $@"SELECT MovieID, COUNT(*)
                              FROM Reviews
                              WHERE MovieID IN ({inClause})
                              GROUP BY MovieID";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);

                for (int index = 0; index < ids.Count; index++)
                {
                    cmd.Parameters.AddWithValue(paramNames[index], ids[index]);
                }

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int movieId = reader.GetInt32(0);
                    int count = reader.GetInt32(1);
                    result[movieId] = count;
                }
            }
            finally
            {
                db.CloseConnection();
            }

            return result;
        }

        public int[] GetStarRatingBuckets(int movieId)
        {
            var counts = new int[StarRatingBucketCount];
            const string query = @"SELECT StarRating FROM Reviews WHERE MovieID = @mid";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);
                cmd.Parameters.AddWithValue("@mid", movieId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var rating = reader.GetInt32(0);
                    var bucket = (int)Math.Floor((double)rating);

                    if (bucket < MinStarRating)
                    {
                        bucket = MinStarRating;
                    }

                    if (bucket > MaxStarRating)
                    {
                        bucket = MaxStarRating;
                    }

                    counts[bucket]++;
                }
            }
            finally
            {
                db.CloseConnection();
            }

            return counts;
        }
    }
}
