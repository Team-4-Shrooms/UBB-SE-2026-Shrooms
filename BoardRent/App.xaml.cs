using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BoardRent.Data;
using BoardRent.Views;
using Microsoft.UI.Xaml.Controls.Primitives;
using BoardRent.Services;
using BoardRent.Repositories;

namespace BoardRent
{
    public partial class App : Application
    {
        public static Window _window;
        private static Frame _rootFrame;

        public App()
        {
            InitializeComponent();
        }
        
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _rootFrame = new Frame();
            _window.Content = _rootFrame;
            _window.Activate();

            // Create DB tables on first launch
            var db = new AppDbContext();
            db.EnsureCreated();

            var dbContext = new AppDbContext();

            //Start at login page
            //NavigateTo(typeof(LoginPage));
            var userRepo = new UserRepository(db);
            var failedLoginRepo = new FailedLoginRepository(dbContext);

            IAuthService authService = new AuthService(userRepo, failedLoginRepo);
            IUserService userService = new UserService(userRepo);

            var loginResult = await authService.LoginMockAsync();

            if (loginResult.Success)
            {
                // Navigate to profile page now that the session is populated
                App.NavigateTo(typeof(ProfilePage));
            }
        }

        public static void NavigateTo(Type pageType)
        {
            _rootFrame?.Navigate(pageType);
        }

        public static void NavigateBack()
        {
            if (_rootFrame != null && _rootFrame.CanGoBack)
            {
                _rootFrame.GoBack();
            }
        }
    }
}
