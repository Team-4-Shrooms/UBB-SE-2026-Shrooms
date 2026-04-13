using System.Collections.Generic;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

namespace MovieShop.Tests.Services
{
    public class WalletServiceTests
    {
        private const int TestUserId = 42;
        private const decimal TestBalance = 123.45m;
        private const decimal NewBalance = 500m;

        private readonly Mock<IUserRepository> userRepoMock = new Mock<IUserRepository>();
        private readonly Mock<ITransactionRepository> transactionRepoMock = new Mock<ITransactionRepository>();

        private WalletService CreateService() =>
            new WalletService(userRepoMock.Object, transactionRepoMock.Object);

        [Fact]
        public void GetBalance_ReturnsBalanceFromUserRepo()
        {
            userRepoMock.Setup(userRepo => userRepo.GetBalance(TestUserId)).Returns(TestBalance);

            decimal result = CreateService().GetBalance(TestUserId);

            Assert.Equal(TestBalance, result);
        }

        [Fact]
        public void GetBalance_DelegatesToUserRepo()
        {
            CreateService().GetBalance(TestUserId);

            userRepoMock.Verify(userRepo => userRepo.GetBalance(TestUserId), Times.Once);
        }

        [Fact]
        public void UpdateBalance_DelegatesToUserRepo()
        {
            CreateService().UpdateBalance(TestUserId, NewBalance);

            userRepoMock.Verify(userRepo => userRepo.UpdateBalance(TestUserId, NewBalance), Times.Once);
        }

        [Fact]
        public void GetTransactionsByUserId_ReturnsTransactionsFromRepo()
        {
            var transactions = new List<Transaction> { new Transaction { ID = 1 } };
            transactionRepoMock
                .Setup(transactionRepo => transactionRepo.GetTransactionsByUserId(TestUserId))
                .Returns(transactions);

            var result = CreateService().GetTransactionsByUserId(TestUserId);

            Assert.Same(transactions, result);
        }

        [Fact]
        public void GetTransactionsByUserId_DelegatesToTransactionRepo()
        {
            CreateService().GetTransactionsByUserId(TestUserId);

            transactionRepoMock.Verify(transactionRepo => transactionRepo.GetTransactionsByUserId(TestUserId), Times.Once);
        }

        [Fact]
        public void LogTransaction_DelegatesToTransactionRepo()
        {
            var transaction = new Transaction { ID = 99 };

            CreateService().LogTransaction(transaction);

            transactionRepoMock.Verify(transactionRepo => transactionRepo.LogTransaction(transaction), Times.Once);
        }
    }
}
