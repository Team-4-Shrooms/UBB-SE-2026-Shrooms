using MovieShop.Models;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public interface ITransactionRepository
    {
        void LogTransaction(Transaction transaction);

        List<Transaction> GetTransactionsByUserId(int userId);
    }
}
