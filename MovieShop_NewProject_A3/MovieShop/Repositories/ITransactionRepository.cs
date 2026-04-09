using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public interface ITransactionRepository
    {
        void LogTransaction(Transaction transaction);

        List<Transaction> GetTransactionsByUserId(int userId);

        void UpdateTransactionStatus(int transactionId, string newStatus);
    }
}
