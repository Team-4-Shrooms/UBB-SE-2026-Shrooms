using System.Threading.Tasks;
using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class TransactionRepoTests
    {
        private readonly TransactionRepo repository;

        public TransactionRepoTests()
        {
            repository = new TransactionRepo();
        }

        [Fact]
        public void GetTransactionsByUserId_ExecutesWithoutException()
        {
            var resultSync = repository.GetTransactionsByUserId(1);
            Assert.NotNull(resultSync);
        }
    }
}
