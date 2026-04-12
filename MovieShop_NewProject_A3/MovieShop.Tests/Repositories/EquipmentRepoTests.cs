using MovieShop.Repositories;

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
        public void FetchAvailableEquipment_ValidCall_ReturnsList()
        {
            var result = repository.FetchAvailableEquipment();
            Assert.NotNull(result);
        }
    }
}
