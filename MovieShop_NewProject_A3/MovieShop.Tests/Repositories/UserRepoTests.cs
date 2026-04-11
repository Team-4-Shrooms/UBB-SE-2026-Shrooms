using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class UserRepoTests
    {
        private readonly UserRepo repository;

        public UserRepoTests()
        {
            repository = new UserRepo();
        }

        [Fact]
        public void GetBalance_ValidUser_NoException()
        {
            // Integration test hitting actual database
            var exception = Record.Exception(() => repository.GetBalance(1));
            Assert.Null(exception);
        }

        [Fact]
        public void UpdateBalance_ValidUser_NoException()
        {
            var exception = Record.Exception(() => repository.UpdateBalance(1, 5000m));
            Assert.Null(exception);
        }
    }
}
