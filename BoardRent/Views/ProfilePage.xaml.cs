using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BoardRent.ViewModels;
using BoardRent.Services;
using BoardRent.Utils;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BoardRent.Views
{
    public sealed partial class ProfilePage : Page
    {
        public ProfileViewModel ViewModel { get; }

        public ProfilePage()
        {
            this.InitializeComponent();

            var userService = Ioc.Default.GetService<IUserService>();
            var authService = Ioc.Default.GetService<IAuthService>();
            ViewModel = new ProfileViewModel(userService, authService);

            this.DataContext = ViewModel;
            this.Loaded += async (s, e) =>
            {
                await ViewModel.LoadProfile();
            };
        }

        private async void SignOut_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SignOut();
            App.NavigateToAndClearBackStack(typeof(LoginPage));
        }
    }
}
