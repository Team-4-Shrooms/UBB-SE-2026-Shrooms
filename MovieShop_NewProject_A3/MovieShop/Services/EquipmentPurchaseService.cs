using System;
using System.Linq;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class EquipmentPurchaseService : IEquipmentPurchaseService
    {
        private readonly IEquipmentRepository equipmentRepo;
        private readonly IUserRepository userRepo;

        public EquipmentPurchaseService(IEquipmentRepository equipmentRepo, IUserRepository userRepo)
        {
            this.equipmentRepo = equipmentRepo;
            this.userRepo = userRepo;
        }

        public bool CanAfford(int userId, decimal price)
        {
            var balance = userRepo.GetBalance(userId);
            SessionManager.CurrentUserBalance = balance;
            return balance >= price;
        }

        public void PurchaseEquipment(int equipmentId, int userId, decimal price, string shippingAddress)
        {
            var balance = userRepo.GetBalance(userId);
            if (balance < price)
            {
                throw new InvalidOperationException(
                    $"Insufficient funds. Balance: {balance:C} — Price: {price:C}");
            }

            equipmentRepo.PurchaseEquipment(equipmentId, userId, price, shippingAddress);

            SessionManager.CurrentUserBalance = userRepo.GetBalance(userId);
        }

        public string ValidateShippingDetails(string name, string address, string phone)
        {
            string error = string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                error += "- Name is required.\n";
            }

            if (address.Length < UIConstants.MinimumAddressLength)
            {
                error += $"- Address too short (min {UIConstants.MinimumAddressLength} chars).\n";
            }

            string trimmedPhone = phone.Trim();
            if (trimmedPhone.Length != UIConstants.PhoneNumberDigitCount || !trimmedPhone.All(char.IsDigit))
            {
                error += $"- Phone must be exactly {UIConstants.PhoneNumberDigitCount} digits.\n";
            }

            return error;
        }
    }
}