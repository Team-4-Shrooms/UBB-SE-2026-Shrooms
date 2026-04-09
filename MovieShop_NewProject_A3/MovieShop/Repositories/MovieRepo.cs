using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public class MovieRepo : IMovieRepository
    {
        private readonly DatabaseSingleton db = DatabaseSingleton.Instance;

        public List<Movie> GetAllMovies()
        {
            var list = new List<Movie>();
            const string query = @"SELECT ID, Title, Description, Price, ImageUrl FROM Movies ORDER BY Title";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(MapMovie(reader));
                }
            }
            finally
            {
                db.CloseConnection();
            }

            PopulateRatingsInCode(list);
            return list;
        }

        public Movie? GetMovieById(int movieId)
        {
            const string query = @"SELECT ID, Title, Description, Price, ImageUrl FROM Movies WHERE ID = @id";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);
                cmd.Parameters.AddWithValue("@id", movieId);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }

                var movie = MapMovie(reader);
                PopulateRatingsInCode(new[] { movie });
                return movie;
            }
            finally
            {
                db.CloseConnection();
            }
        }

        public bool UserOwnsMovie(int userId, int movieId)
        {
            if (userId <= 0)
            {
                return false;
            }

            const string query = @"SELECT 1 FROM OwnedMovies WHERE UserID = @uid AND MovieID = @mid";

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@mid", movieId);
                var o = cmd.ExecuteScalar();
                return o != null && o != DBNull.Value;
            }
            finally
            {
                db.CloseConnection();
            }
        }

        public void PurchaseMovie(int userId, int movieId, decimal finalPrice)
        {
            if (userId <= 0)
            {
                throw new InvalidOperationException("You must be logged in to purchase.");
            }

            db.OpenConnection();
            using var sqlTrans = db.Connection.BeginTransaction();
            try
            {
                int ownedCount;
                string checkOwned = @"SELECT COUNT(*) FROM OwnedMovies WHERE UserID = @uid AND MovieID = @mid";
                using (var cmd = new SqlCommand(checkOwned, db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    ownedCount = (int)cmd.ExecuteScalar() !;
                }

                if (ownedCount > 0)
                {
                    throw new InvalidOperationException("You already own this movie.");
                }

                string deductSql = @"UPDATE Users SET Balance = Balance - @price WHERE ID = @uid AND Balance >= @price";
                int updated;
                using (var cmd = new SqlCommand(deductSql, db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@price", finalPrice);
                    cmd.Parameters.AddWithValue("@uid", userId);
                    updated = cmd.ExecuteNonQuery();
                }

                if (updated == 0)
                {
                    throw new InvalidOperationException("Insufficient balance.");
                }

                string insertOwned = @"INSERT INTO OwnedMovies (UserID, MovieID) VALUES (@uid, @mid)";
                using (var cmd = new SqlCommand(insertOwned, db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@mid", movieId);
                    cmd.ExecuteNonQuery();
                }

                string insertTx = @"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, MovieID, EventID, Amount, Type, Status, Timestamp, ShippingAddress) VALUES (@buyerID, NULL, NULL, @movieID, NULL, @amount, @type, @status, @timestamp, NULL)";
                using (var cmd = new SqlCommand(insertTx, db.Connection, sqlTrans))
                {
                    cmd.Parameters.AddWithValue("@buyerID", userId);
                    cmd.Parameters.AddWithValue("@movieID", movieId);
                    cmd.Parameters.AddWithValue("@amount", -finalPrice);
                    cmd.Parameters.AddWithValue("@type", "MoviePurchase");
                    cmd.Parameters.AddWithValue("@status", "Completed");
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }

                sqlTrans.Commit();
            }
            catch
            {
                sqlTrans.Rollback();
                throw;
            }
            finally
            {
                db.CloseConnection();
            }
        }
        private static Movie MapMovie(SqlDataReader reader)
        {
            return new Movie
            {
                ID = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                Price = reader.GetDecimal(3),
                ImageUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }

        private void PopulateRatingsInCode(IReadOnlyList<Movie> movies)
        {
            if (movies.Count == 0)
            {
                return;
            }

            var ids = movies.Select(m => m.ID).Distinct().ToList();

            var paramNames = ids.Select((_, i) => $"@id{i}").ToArray();
            var inClause = string.Join(",", paramNames);

            var query = $@"SELECT MovieID, AVG(CAST(StarRating AS FLOAT)) AS AvgRating, COUNT(*) AS ReviewCount
            FROM Reviews
            WHERE MovieID IN ({inClause})
            GROUP BY MovieID";

            var ratings = new Dictionary<int, double>();

            db.OpenConnection();
            try
            {
                using var cmd = new SqlCommand(query, db.Connection);

                for (int i = 0; i < ids.Count; i++)
                {
                    cmd.Parameters.AddWithValue(paramNames[i], ids[i]);
                }

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int movieId = reader.GetInt32(0);
                    double avg = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);

                    ratings[movieId] = avg;
                }
            }
            finally
            {
                db.CloseConnection();
            }

            foreach (var m in movies)
            {
                if (ratings.TryGetValue(m.ID, out var avg))
                {
                    m.Rating = avg;
                }
                else
                {
                    m.Rating = 0;
                }
            }
        }
    }
}
