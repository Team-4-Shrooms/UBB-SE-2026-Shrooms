using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class EquipmentRepoTests
    {
        private readonly EquipmentRepo repo;

        public EquipmentRepoTests()
        {
            repo = new EquipmentRepo();
        }

        [Fact]
        public void FetchAvailableEquipment_ExecutesAndReturnsList()
        {
            // Integration test
            var result = repo.FetchAvailableEquipment();
            Assert.NotNull(result);
        }
    }
}
