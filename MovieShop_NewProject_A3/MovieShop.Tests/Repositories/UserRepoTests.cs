using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class UserRepoTests
    {
        private const int TestUserId = 1;
        private const decimal TestBalance = 5000m;

        private readonly UserRepo repository;

        public UserRepoTests()
        {
            repository = new UserRepo();
        }

        [Fact]
        public void GetBalance_ValidUser_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.GetBalance(TestUserId));
            Assert.Null(exception);
        }

        [Fact]
        public void UpdateBalance_ValidUser_ExecutesWithoutException()
        {
            var exception = Record.Exception(() => repository.UpdateBalance(TestUserId, TestBalance));
            Assert.Null(exception);
        }
    }
}
