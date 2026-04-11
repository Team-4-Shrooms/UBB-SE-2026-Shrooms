using System.Collections.Generic;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUserRepository userRepo;
        private readonly ITransactionRepository transactionRepo;

        public WalletService(IUserRepository userRepo, ITransactionRepository transactionRepo)
        {
            this.userRepo = userRepo;
            this.transactionRepo = transactionRepo;
        }

        public decimal GetBalance(int userId)
        {
            return userRepo.GetBalance(userId);
        }

        public void UpdateBalance(int userId, decimal newBalance)
        {
            userRepo.UpdateBalance(userId, newBalance);
        }

        public List<Transaction> GetTransactionsByUserId(int userId)
        {
            return transactionRepo.GetTransactionsByUserId(userId);
        }

        public void LogTransaction(Transaction transaction)
        {
            transactionRepo.LogTransaction(transaction);
        }
    }
}
