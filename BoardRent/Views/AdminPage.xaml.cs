using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BoardRent.Services;
using BoardRent.ViewModels;
using BoardRent.Utils;
using BoardRent.Repositories;
using BoardRent.Data;
using System.ComponentModel;

namespace BoardRent.Views
{
    public sealed partial class AdminPage : Page, INotifyPropertyChanged
    {
        public AdminViewModel ViewModel { get; }

        public AdminPage()
        {
            InitializeComponent();
            
            // Temporary manual DI wire-up (repositories currently blocked/wip)
            // Replace with proper Dependency Injection as project evolves.
            var dbContext = new AppDbContext();
            var userRepository = new UserRepository(dbContext);
            var failedLoginRepository = new FailedLoginRepository(dbContext);
            var adminService = new AdminService(userRepository, failedLoginRepository);
            
            ViewModel = new AdminViewModel(adminService);
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public bool IsUnauthorized => !SessionContext.GetInstance().IsLoggedIn || SessionContext.GetInstance().Role != "Administrator";
        
        public Visibility IsAuthorizedVisibility => IsUnauthorized ? Visibility.Collapsed : Visibility.Visible;

        public bool IsErrorVisible => !string.IsNullOrEmpty(ViewModel.ErrorMessage);

        public event PropertyChangedEventHandler PropertyChanged;

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AdminViewModel.ErrorMessage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsErrorVisible)));
            }
        }

        private void OnSignOutClicked(object sender, RoutedEventArgs e)
        {
            SessionContext.GetInstance().Clear();
            App.NavigateTo(typeof(LoginPage));
        }
    }
}
