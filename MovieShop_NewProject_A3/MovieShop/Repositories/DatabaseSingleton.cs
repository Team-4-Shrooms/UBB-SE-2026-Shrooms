using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace MovieShop.Repositories
{
    public sealed class DatabaseSingleton : IDatabaseSingleton
    {
        private static DatabaseSingleton? instance;
        private readonly SqlConnection connection;

        public static DatabaseSingleton Instance => instance ??= new DatabaseSingleton();

        public SqlConnection Connection => connection;
        private DatabaseSingleton() => connection = new SqlConnection($"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={Helpers.GetProjectDirectory()}\\MovieShopDB.mdf;Integrated Security=True;MultipleActiveResultSets=True");

        public void OpenConnection()
        {
            if (connection.State == System.Data.ConnectionState.Open)
            {
                return;
            }

            connection.Open();
        }

        public void CloseConnection() => connection.Close();
    }
}
