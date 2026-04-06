using Microsoft.Data.SqlClient;
using MovieShop.Models;
using System;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public class ActiveSalesRepo : IActiveSalesRepository
    {
        DatabaseSingleton _database = DatabaseSingleton.Instance; 

        public Dictionary<int, decimal> GetBestDiscountPercentByMovieId()
        {
            var map = new Dictionary<int, decimal>();
            foreach (var sale in GetCurrentSales())
            {
                var id = sale.Movie.ID;
                var percentage = sale.DiscountPercentage; 
                if (!map.TryGetValue(id, out var existing) || percentage > existing)
                    map[id] = percentage;
            }

            return map;
        }

        public static void ApplyBestDiscountsToMovies(IReadOnlyList<Movie> movies, Dictionary<int, decimal> bestDiscountByMovieId)
        {
            foreach (var movie in movies) 
            {
                if (bestDiscountByMovieId.TryGetValue(movie.ID, out var percentage)) 
                    movie.ActiveSaleDiscountPercent = percentage;
                else
                    movie.ActiveSaleDiscountPercent = null;
            }
        }

        public List<ActiveSale> GetCurrentSales()
        {
            List<ActiveSale> sales = new List<ActiveSale>();

            string query = @"SELECT s.ID, s.DiscountPercentage, s.EndTime, m.ID AS MovieID, m.Title, m.Price
                            FROM ActiveSales s
                            JOIN Movies m ON s.MovieID = m.ID
                            WHERE s.StartTime <= GETDATE() AND s.EndTime > GETDATE()
                            ORDER BY s.EndTime ASC";

            SqlCommand command = new SqlCommand(query, _database.Connection); 
            _database.OpenConnection();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    sales.Add(new ActiveSale
                    {
                        ID = (int)reader["ID"],
                        DiscountPercentage = (decimal)reader["DiscountPercentage"],
                        EndTime = (DateTime)reader["EndTime"],
                        Movie = new Movie
                        {
                            ID = (int)reader["MovieID"],
                            Title = reader["Title"].ToString() ?? "<no title>",
                            Price = (decimal)reader["Price"]
                        }
                    });
                }
            }

            _database.CloseConnection();
            return sales;
        }
    }
}