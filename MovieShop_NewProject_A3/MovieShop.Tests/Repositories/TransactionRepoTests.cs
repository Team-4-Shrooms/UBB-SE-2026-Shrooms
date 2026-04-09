using System.Threading.Tasks;
using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class TransactionRepoTests
    {
        private readonly TransactionRepo repo;

        public TransactionRepoTests()
        {
            repo = new TransactionRepo();
        }

        [Fact]
        public void GetTransactionsByUserId_ExecutesWithoutException()
        {
            var resultSync = repo.GetTransactionsByUserId(1);
            Assert.NotNull(resultSync);
        }
    }
}
