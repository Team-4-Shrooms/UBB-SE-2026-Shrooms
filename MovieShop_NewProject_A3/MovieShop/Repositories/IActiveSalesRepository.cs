using System.Collections.Generic;
using MovieShop.Models;

namespace MovieShop.Repositories
{
    public interface IActiveSalesRepository
    {
        Dictionary<int, decimal> GetBestDiscountPercentByMovieId();

        List<ActiveSale> GetCurrentSales();
    }
}
