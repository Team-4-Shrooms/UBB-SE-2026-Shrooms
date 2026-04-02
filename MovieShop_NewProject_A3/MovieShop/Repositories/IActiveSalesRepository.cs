using MovieShop.Models;
using System.Collections.Generic;

namespace MovieShop.Repositories
{
    public interface IActiveSalesRepository
    {
        Dictionary<int, decimal> GetBestDiscountPercentByMovieId();

        List<ActiveSale> GetCurrentSales();
    }
}
