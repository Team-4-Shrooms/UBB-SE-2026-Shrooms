using MovieShop.Repositories;
using Xunit;

namespace MovieShop.Tests.Repositories
{
    public class EquipmentRepoTests
    {
        private readonly EquipmentRepo _repo;

        public EquipmentRepoTests()
        {
            _repo = new EquipmentRepo();
        }

        [Fact]
        public void FetchAvailableEquipment_ExecutesAndReturnsList()
        {
            // Integration test
            var result = _repo.FetchAvailableEquipment();
            Assert.NotNull(result);
        }
    }
}
