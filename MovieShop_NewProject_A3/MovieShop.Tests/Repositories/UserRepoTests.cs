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

        [Fact]
        public void GetBalance_InvalidUserId_ReturnsZero()
        {
            Assert.Equal(0m, repository.GetBalance(0));
            Assert.Equal(0m, repository.GetBalance(-5));
        }

        [Fact]
        public void UpdateBalance_InvalidUserId_DoesNotThrow()
        {
            var exception = Record.Exception(() => repository.UpdateBalance(0, TestBalance));
            Assert.Null(exception);
        }
    }
}
