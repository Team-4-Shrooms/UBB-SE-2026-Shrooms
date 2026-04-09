using MovieShop.ViewModels;

namespace MovieShop.Services
{
    public interface ISaleService
    {
        FlashSaleViewModel? CurrentSale { get; set; }
    }
}
