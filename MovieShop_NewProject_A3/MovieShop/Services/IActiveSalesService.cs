using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Services
{
    public interface IActiveSalesService
    {
        List<ActiveSale> GetCurrentSales();
    }
}
