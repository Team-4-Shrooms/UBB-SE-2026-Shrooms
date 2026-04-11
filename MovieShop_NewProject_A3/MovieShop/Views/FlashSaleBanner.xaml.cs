using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MovieShop.ViewModels;

namespace MovieShop.Views
{
    public sealed partial class FlashSaleBanner : UserControl
    {
        public FlashSaleBannerViewModel ViewModel { get; } = App.Services.GetRequiredService<FlashSaleBannerViewModel>();

        public FlashSaleBanner()
        {
            InitializeComponent();
        }
    }
}
