using System.Collections.Generic;
using Moq;
using MovieShop.Models;
using MovieShop.Repositories;
using MovieShop.Services;

namespace MovieShop.Tests.Services
{
    public class MarketplaceServiceTests
    {
        private const string CategoryCamera = "Camera";
        private const string CategoryLighting = "Lighting";
        private const string AllCategoriesLabel = "All";

        private readonly Mock<IEquipmentRepository> equipmentRepoMock = new Mock<IEquipmentRepository>();

        private MarketplaceService CreateService() => new MarketplaceService(equipmentRepoMock.Object);

        [Fact]
        public void GetAvailableEquipment_ReturnsRepositoryResult()
        {
            var items = new List<Equipment> { new Equipment { ID = 1 } };
            equipmentRepoMock.Setup(equipmentRepo => equipmentRepo.FetchAvailableEquipment()).Returns(items);

            var result = CreateService().GetAvailableEquipment();

            Assert.Same(items, result);
        }

        [Fact]
        public void GetAvailableEquipment_RepositoryReturnsNull_ReturnsEmptyList()
        {
            equipmentRepoMock.Setup(equipmentRepo => equipmentRepo.FetchAvailableEquipment()).Returns((List<Equipment>?)null);

            var result = CreateService().GetAvailableEquipment();

            Assert.Empty(result);
        }

        [Fact]
        public void FilterByCategory_NullCategory_ReturnsAllItems()
        {
            var items = new List<Equipment> { new Equipment { Category = CategoryCamera }, new Equipment { Category = CategoryLighting } };

            var result = CreateService().FilterByCategory(items, null);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FilterByCategory_EmptyCategory_ReturnsAllItems()
        {
            var items = new List<Equipment> { new Equipment { Category = CategoryCamera }, new Equipment { Category = CategoryLighting } };

            var result = CreateService().FilterByCategory(items, string.Empty);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FilterByCategory_AllCategoriesLabel_ReturnsAllItems()
        {
            var items = new List<Equipment> { new Equipment { Category = CategoryCamera }, new Equipment { Category = CategoryLighting } };

            var result = CreateService().FilterByCategory(items, AllCategoriesLabel);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FilterByCategory_SpecificCategory_ReturnsOnlyMatchingItems()
        {
            var items = new List<Equipment>
            {
                new Equipment { Category = CategoryCamera },
                new Equipment { Category = CategoryLighting },
                new Equipment { Category = CategoryCamera },
            };

            var result = CreateService().FilterByCategory(items, CategoryCamera);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void FilterByCategory_NoMatches_ReturnsEmptyList()
        {
            var items = new List<Equipment> { new Equipment { Category = CategoryCamera } };

            var result = CreateService().FilterByCategory(items, CategoryLighting);

            Assert.Empty(result);
        }

        [Fact]
        public void ListItem_DelegatesToRepository()
        {
            var item = new Equipment { ID = 7 };

            CreateService().ListItem(item);

            equipmentRepoMock.Verify(equipmentRepo => equipmentRepo.ListItem(item), Times.Once);
        }
    }
}
