using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class TransactionRepoTests
    {
        private const int TestUserId = 1;

        private readonly TransactionRepo repository;

        public TransactionRepoTests()
        {
            repository = new TransactionRepo();
        }

        [Fact]
        public void GetTransactionsByUserId_ValidUser_ReturnsResult()
        {
            var result = repository.GetTransactionsByUserId(TestUserId);
            Assert.NotNull(result);
        }
    }
}
