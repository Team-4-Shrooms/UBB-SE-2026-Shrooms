using Microsoft.Data.SqlClient;

namespace MovieShop.Repositories
{
    public interface IDatabaseSingleton
    {
        SqlConnection Connection { get; }

        void OpenConnection();

        void CloseConnection();
    }
}
