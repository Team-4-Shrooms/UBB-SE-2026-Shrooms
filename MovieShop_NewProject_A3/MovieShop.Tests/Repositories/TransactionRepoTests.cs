using MovieShop.Repositories;
using Xunit;
using System.Threading.Tasks;

namespace MovieShop.Tests.Repositories
{
    public class TransactionRepoTests
    {
        private readonly TransactionRepo _repo;

        public TransactionRepoTests()
        {
            _repo = new TransactionRepo();
        }

        [Fact]
        public void GetTransactionsByUserId_ExecutesWithoutException()
        {
            var resultSync = _repo.GetTransactionsByUserId(1);
            Assert.NotNull(resultSync);
        }
    }
}
