using MovieShop.Models;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public interface IEquipmentRepository
    {
        List<Equipment> FetchAvailableEquipment();

        void ListItem(Equipment item);

        void PurchaseEquipment(int equipmentId, int buyerId, decimal price, string address);
    }
}
