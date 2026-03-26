using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using BoardRent.ViewModels;
using BoardRent.Services;
using BoardRent.Utils;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace BoardRent.Views
{
    public sealed partial class AdminPage : Page, INotifyPropertyChanged
    {
        public AdminViewModel ViewModel { get; }

        public AdminPage()
        {
            InitializeComponent();
            var adminService = Ioc.Default.GetService<IAdminService>();
            ViewModel = new AdminViewModel(adminService);
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        public bool IsUnauthorized => !SessionContext.GetInstance().IsLoggedIn || SessionContext.GetInstance().Role != "Administrator";
        public Visibility IsAuthorizedVisibility => IsUnauthorized ? Visibility.Collapsed : Visibility.Visible;
        public bool IsErrorVisible => ViewModel != null && !string.IsNullOrEmpty(ViewModel.ErrorMessage);
        public bool IsSuccessVisible => ViewModel != null && !string.IsNullOrEmpty(ViewModel.SuccessMessage);

        public event PropertyChangedEventHandler PropertyChanged;

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AdminViewModel.ErrorMessage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsErrorVisible)));
            }
            if (e.PropertyName == nameof(AdminViewModel.SuccessMessage))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSuccessVisible)));
            }
        }

        private void OnSignOutClicked(object sender, RoutedEventArgs e)
        {
            SessionContext.GetInstance().Clear();
            App.NavigateToAndClearBackStack(typeof(LoginPage));
        }
    }
}
