using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using MovieShop.Repositories;
using MovieShop.Services;
using MovieShop.ViewModels;

namespace MovieShop
{
    public partial class App : Application
    {
        public static Window? CurrentWindow;

        public static IServiceProvider Services { get; private set; }

        public App()
        {
            InitializeComponent();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            Services = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IDatabaseSingleton>(DatabaseSingleton.Instance);
            services.AddSingleton<ISaleService, SaleService>();

            services.AddTransient<IUserRepository, UserRepo>();
            services.AddTransient<IMovieRepository, MovieRepo>();
            services.AddTransient<IEquipmentRepository, EquipmentRepo>();
            services.AddTransient<IEventRepository, EventRepo>();
            services.AddTransient<IActiveSalesRepository, ActiveSalesRepo>();
            services.AddTransient<IReviewRepository, ReviewRepo>();
            services.AddTransient<ITransactionRepository, TransactionRepo>();
            services.AddTransient<IInventoryRepository, InventoryRepo>();

            services.AddTransient<IMoviePurchaseService, MoviePurchaseService>();
            services.AddTransient<IMovieReviewService, MovieReviewService>();
            services.AddTransient<IMovieCatalogService, MovieCatalogService>();
            services.AddTransient<IInventoryService, InventoryService>();
            services.AddTransient<IEquipmentPurchaseService, EquipmentPurchaseService>();
            services.AddTransient<IEventTicketService, EventTicketService>();
            services.AddTransient<IMarketplaceService, MarketplaceService>();
            services.AddTransient<IWalletService, WalletService>();
            services.AddTransient<IActiveSalesService, ActiveSalesService>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<MarketplaceViewModel>();
            services.AddTransient<SellEquipmentViewModel>();
            services.AddTransient<MovieReviewsViewModel>();
            services.AddTransient<MovieDetailViewModel>();
            services.AddTransient<BuyTicketViewModel>();
            services.AddTransient<MovieEventsViewModel>();
            services.AddTransient<InventoryViewModel>();
            services.AddTransient<MovieCatalogViewModel>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<EquipmentDetailViewModel>();
            services.AddTransient<FlashSaleBannerViewModel>();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            CurrentWindow = new MainWindow();
            CurrentWindow.Activate();
        }
    }
}
