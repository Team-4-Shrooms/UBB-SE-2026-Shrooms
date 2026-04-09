using MovieShop.ViewModels;

namespace MovieShop.Services
{
    public class SaleService : ISaleService
    {
        public FlashSaleViewModel? CurrentSale { get; set; }
    }
}