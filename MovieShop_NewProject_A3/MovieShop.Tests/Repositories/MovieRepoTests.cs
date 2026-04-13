using System;
using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class MovieRepoTests
    {
        private readonly MovieRepo repository;

        public MovieRepoTests()
        {
            repository = new MovieRepo();
        }

        [Fact]
        public void GetAllMovies_ValidCall_ReturnsList()
        {
            var result = repository.GetAllMovies();
            Assert.NotNull(result);
        }

        [Fact]
        public void UserOwnsMovie_InvalidUserId_ReturnsFalse()
        {
            Assert.False(repository.UserOwnsMovie(0, 1));
            Assert.False(repository.UserOwnsMovie(-1, 1));
        }

        [Fact]
        public void PurchaseMovie_InvalidUserId_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() => repository.PurchaseMovie(0, 1, 9.99m));
        }
    }
}
