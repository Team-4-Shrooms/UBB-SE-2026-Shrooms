using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Services
{
    public interface IWalletService
    {
        decimal GetBalance(int userId);

        void UpdateBalance(int userId, decimal newBalance);

        List<Transaction> GetTransactionsByUserId(int userId);

        void LogTransaction(Transaction transaction);
    }
}
