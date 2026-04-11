using MovieShop.Services;

namespace MovieShop.ViewModels
{
    public class FlashSaleBannerViewModel
    {
        private readonly ISaleService saleService;

        public FlashSaleBannerViewModel(ISaleService saleService)
        {
            this.saleService = saleService;
        }

        public FlashSaleViewModel? CurrentSale => saleService.CurrentSale;
    }
}
