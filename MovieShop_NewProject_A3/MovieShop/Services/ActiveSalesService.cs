using System.Collections.Generic;
using MovieShop.Models;
using MovieShop.Repositories;

namespace MovieShop.Services
{
    public class ActiveSalesService : IActiveSalesService
    {
        private readonly IActiveSalesRepository salesRepo;

        public ActiveSalesService(IActiveSalesRepository salesRepo)
        {
            this.salesRepo = salesRepo;
        }

        public List<ActiveSale> GetCurrentSales()
        {
            return salesRepo.GetCurrentSales();
        }
    }
}
