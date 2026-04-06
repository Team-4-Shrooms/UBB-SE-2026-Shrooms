using Microsoft.Data.SqlClient;
using MovieShop.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MovieShop.Repositories
{
    public class EquipmentRepo : IEquipmentRepository
    {
        private const string StatusAvailable = "Available";
        private const string StatusSold = "Sold";
        private const string StatusCompleted = "Completed";

        DatabaseSingleton _database = DatabaseSingleton.Instance;

        public List<Equipment> FetchAvailableEquipment()
        {
            var items = new List<Equipment>();

            string query = $"SELECT ID, SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition FROM Equipment WHERE Status = '{StatusAvailable}'";

            SqlCommand command = new SqlCommand(query, _database.Connection);

            try
            {
                _database.OpenConnection();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new Equipment
                        {
                            ID = reader.GetInt32(0),
                            SellerID = reader.GetInt32(1),
                            Title = reader.GetString(2),
                            Price = reader.GetDecimal(3),
                            Status = EquipmentStatus.Available,
                            Description = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            ImageUrl = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            Category = reader.IsDBNull(7) ? "" : reader.GetString(7),
                            Condition = reader.IsDBNull(8) ? "" : reader.GetString(8)
                        });
                    }
                }
                _database.CloseConnection();
            }
            catch (Exception exception)
            {
                _database.CloseConnection();
                Debug.WriteLine("Fetch error: " + exception.Message);
                throw;
            }

            return items;
        }

        public void ListItem(Equipment item)
        {
            string query = $@"INSERT INTO Equipment (SellerID, Title, Price, Status, Description, ImageUrl, Category, Condition) 
                            VALUES (@sellerId, @title, @price, '{StatusAvailable}', @description, @imageUrl, @category, @condition)";

            SqlCommand command = new SqlCommand(query, _database.Connection);
            command.Parameters.AddWithValue("@sellerId", item.SellerID);
            command.Parameters.AddWithValue("@title", item.Title);
            command.Parameters.AddWithValue("@price", item.Price);
            command.Parameters.AddWithValue("@category", item.Category ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@condition", item.Condition ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@description", string.IsNullOrEmpty(item.Description) ? (object)DBNull.Value : item.Description);
            command.Parameters.AddWithValue("@imageUrl", string.IsNullOrEmpty(item.ImageUrl) ? (object)DBNull.Value : item.ImageUrl);

            _database.OpenConnection();
            command.ExecuteNonQuery();
            _database.CloseConnection();
        }

        public void PurchaseEquipment(int equipmentId, int buyerId, decimal price, string address)
        {
            _database.OpenConnection();
            SqlTransaction sqlTransaction = _database.Connection.BeginTransaction();

            try
            {
                string deductQuery = "UPDATE Users SET Balance = Balance - @price WHERE ID = @buyerId";
                SqlCommand deductCommand = new SqlCommand(deductQuery, _database.Connection, sqlTransaction);
                deductCommand.Parameters.AddWithValue("@price", price);
                deductCommand.Parameters.AddWithValue("@buyerId", buyerId);
                deductCommand.ExecuteNonQuery();

                string updateEquipmentQuery = $"UPDATE Equipment SET Status = '{StatusSold}' WHERE ID = @equipmentId";
                SqlCommand updateEquipmentCommand = new SqlCommand(updateEquipmentQuery, _database.Connection, sqlTransaction);
                updateEquipmentCommand.Parameters.AddWithValue("@equipmentId", equipmentId);
                updateEquipmentCommand.ExecuteNonQuery();

                string logTransactionQuery = $@"INSERT INTO Transactions (BuyerID, SellerID, EquipmentID, Amount, Status, ShippingAddress, Type, Timestamp) 
                                              SELECT @buyerId, SellerID, ID, @amount, '{StatusCompleted}', @address, 'EquipmentPurchase', GETDATE()
                                              FROM Equipment WHERE ID = @equipmentId";

                SqlCommand logTransactionCommand = new SqlCommand(logTransactionQuery, _database.Connection, sqlTransaction);
                logTransactionCommand.Parameters.AddWithValue("@buyerId", buyerId);
                logTransactionCommand.Parameters.AddWithValue("@amount", -price);
                logTransactionCommand.Parameters.AddWithValue("@address", address);
                logTransactionCommand.Parameters.AddWithValue("@equipmentId", equipmentId);
                logTransactionCommand.ExecuteNonQuery();

                sqlTransaction.Commit();
                _database.CloseConnection();
            }
            catch (Exception exception)
            {
                sqlTransaction.Rollback();
                _database.CloseConnection();
                Debug.WriteLine("Failed transaction: " + exception.Message);
                throw;
            }
        }
    }
}