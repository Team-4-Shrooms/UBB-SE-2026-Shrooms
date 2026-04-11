using System.Collections.Generic;
using System.Linq;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class MarketplaceService : IMarketplaceService
    {
        private const string AllCategoriesLabel = "All";

        private readonly IEquipmentRepository equipmentRepo;

        public MarketplaceService(IEquipmentRepository equipmentRepo)
        {
            this.equipmentRepo = equipmentRepo;
        }

        public List<Equipment> GetAvailableEquipment()
        {
            return equipmentRepo.FetchAvailableEquipment() ?? new List<Equipment>();
        }

        public List<Equipment> FilterByCategory(IEnumerable<Equipment> items, string? category)
        {
            if (string.IsNullOrEmpty(category) || category == AllCategoriesLabel)
            {
                return items.ToList();
            }

            return items.Where(item => item.Category == category).ToList();
        }

        public void ListItem(Equipment item)
        {
            equipmentRepo.ListItem(item);
        }
    }
}
