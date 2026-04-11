using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class EquipmentRepoTests
    {
        private readonly EquipmentRepo repository;

        public EquipmentRepoTests()
        {
            repository = new EquipmentRepo();
        }

        [Fact]
        public void FetchAvailableEquipment_ExecutesAndReturnsList()
        {
            // Integration test
            var result = repository.FetchAvailableEquipment();
            Assert.NotNull(result);
        }
    }
}
