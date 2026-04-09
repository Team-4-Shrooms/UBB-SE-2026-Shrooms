using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop.Views
{
    public sealed partial class FlashSaleBanner : UserControl
    {
        private readonly ISaleService saleService = App.Services.GetRequiredService<ISaleService>();

        public FlashSaleViewModel? ViewModel => saleService.CurrentSale;

        public FlashSaleBanner()
        {
            InitializeComponent();
        }
    }
}
