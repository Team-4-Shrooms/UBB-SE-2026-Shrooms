using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Services
{
    public interface IMarketplaceService
    {
        List<Equipment> GetAvailableEquipment();

        List<Equipment> FilterByCategory(IEnumerable<Equipment> items, string? category);

        void ListItem(Equipment item);
    }
}
