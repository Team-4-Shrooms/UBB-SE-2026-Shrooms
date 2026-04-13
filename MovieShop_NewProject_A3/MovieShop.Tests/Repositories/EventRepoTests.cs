using System;
using MovieShop.Repositories;

namespace MovieShop.Tests.Repositories
{
    public class EventRepoTests
    {
        private readonly EventRepo repository;

        public EventRepoTests()
        {
            repository = new EventRepo();
        }

        [Fact]
        public void GetAllEvents_ValidCall_ReturnsList()
        {
            var result = repository.GetAllEvents();
            Assert.NotNull(result);
        }

        [Fact]
        public void PurchaseTicket_InvalidUserId_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() => repository.PurchaseTicket(0, 1));
            Assert.Throws<InvalidOperationException>(() => repository.PurchaseTicket(-1, 1));
        }
    }
}
