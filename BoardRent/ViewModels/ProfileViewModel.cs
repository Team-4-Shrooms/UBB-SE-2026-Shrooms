using BoardRent.DTOs;
using BoardRent.Services;
using BoardRent.Utils;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using WinRT.Interop;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using BoardRent.Views;

namespace BoardRent.ViewModels
{
    public class ProfileViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public ICommand SaveProfileCommand { get; }
        public ICommand UploadAvatarCommand { get; }

        public ICommand RemoveAvatarCommand { get; }

        public ICommand SaveNewPasswordCommand { get; }

        public ProfileViewModel(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
            SaveProfileCommand = new RelayCommand(async () => await SaveProfile());
            RemoveAvatarCommand = new RelayCommand(async () => await RemoveAvatar());
            UploadAvatarCommand = new RelayCommand(async () => await UploadAvatar());
            SaveNewPasswordCommand = new RelayCommand(async () => await SaveNewPassword());
            //LoadProfile();
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        private string _username;
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        private string _displayName;
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }

        private string _email;
        public string Email { get => _email; set => SetProperty(ref _email, value); }

        private string _phoneNumber;
        public string PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }

        private string _country;
        public string Country { get => _country; set => SetProperty(ref _country, value); }

        private string _city;
        public string City { get => _city; set => SetProperty(ref _city, value); }

        private string _streetName;
        public string StreetName { get => _streetName; set => SetProperty(ref _streetName, value); }

        private string _streetNumber;
        public string StreetNumber { get => _streetNumber; set => SetProperty(ref _streetNumber, value); }

        private string _avatarUrl;
        public string AvatarUrl { get => _avatarUrl; set => SetProperty(ref _avatarUrl, value); }

        private string _currentPassword;
        public string CurrentPassword { get => _currentPassword; set => SetProperty(ref _currentPassword, value); }

        private string _newPassword;
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private string _confirmPassword;
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

        private string _error;
        public string ErrorMessage { get => _error; set => SetProperty(ref _error, value); }

        private string _profileMessage;
        public string ProfileMessage { get => _profileMessage; set => SetProperty(ref _profileMessage, value); }

        private string _profileError;
        public string ProfileError { get => _profileError; set => SetProperty(ref _profileError, value); }

        public async Task LoadProfile()
        {
            var userId = SessionContext.GetInstance().UserId;
            Debug.WriteLine($"Session UserId: {userId}");
            var result = await _userService.GetProfileAsync(userId);
            Debug.WriteLine($"Session UserId: {result}");
            Debug.WriteLine($"Result username: {result.Success}");

            if (result.Data != null)
            {
                Username = result.Data.Username;
                Debug.WriteLine("INSIDE IF");
                DisplayName = result.Data.DisplayName;
                Email = result.Data.Email;
                PhoneNumber = result.Data.PhoneNumber;
                Country = result.Data.Country;
                City = result.Data.City;
                StreetName = result.Data.StreetName;
                StreetNumber = result.Data.StreetNumber;
                AvatarUrl = result.Data.AvatarUrl;
            }
        }

        private async Task SaveProfile()
        {
            ProfileMessage = string.Empty;
            ProfileError = string.Empty;

            if (string.IsNullOrWhiteSpace(DisplayName) || DisplayName.Trim().Length < 2 || DisplayName.Trim().Length > 50)
            {
                ProfileError = "Display name must be between 2 and 50 characters.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ProfileError = "Email is required.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(PhoneNumber) && !Regex.IsMatch(PhoneNumber.Trim(), @"^\+?[0-9\s\-()]{7,15}$"))
            {
                ProfileError = "Please enter a valid phone number.";
                return;
            }

            var userId = SessionContext.GetInstance().UserId;

            var dto = new UserProfileDto
            {
                Id = userId,
                Username = Username,
                DisplayName = DisplayName,
                Email = Email,
                PhoneNumber = PhoneNumber,
                Country = Country,
                City = City,
                StreetName = StreetName,
                StreetNumber = StreetNumber
            };

            var result = await _userService.UpdateProfileAsync(userId, dto);

            if (result.Success)
            {
                ProfileMessage = "Profile updated successfully.";
            }
            else
            {
                ProfileError = result.Error ?? "Failed to update profile.";
            }
        }

        public async Task<string> UploadAvatar(Guid userId, string filePath)
        {

            return await _userService.UploadAvatarAsync(userId, filePath);
        }

        public async Task UploadAvatar()
        {
            var picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle(App._window);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            var userId = SessionContext.GetInstance().UserId;
            var savedPath = await _userService.UploadAvatarAsync(userId, file.Path);

            AvatarUrl = savedPath;

            
        }

        public async Task RemoveAvatar()
        {
            var userId = SessionContext.GetInstance().UserId;
            await _userService.RemoveAvatarAsync(userId);
            AvatarUrl = null;
        }

        public async Task SignOut()
        {
            await _authService.LogoutAsync();

            Username = null;
            DisplayName = null;
            Email = null;
            AvatarUrl = null;
        }

        public async Task SaveNewPassword()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                ErrorMessage = "Please enter your current password.";
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "Please enter a new password.";
                return;
            }

            if (NewPassword.Length < 8)
            {
                ErrorMessage = "Password must be at least 8 characters long.";
                return;
            }

            if (!Regex.IsMatch(NewPassword, @"[A-Z]"))
            {
                ErrorMessage = "Password must contain at least one uppercase letter.";
                return;
            }

            if (!Regex.IsMatch(NewPassword, @"[0-9]"))
            {
                ErrorMessage = "Password must contain at least one number.";
                return;
            }

            if (!Regex.IsMatch(NewPassword, @"[^a-zA-Z0-9]"))
            {
                ErrorMessage = "Password must contain at least one special character.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            var userId = SessionContext.GetInstance().UserId;
            var result = await _userService.ChangePasswordAsync(userId, CurrentPassword, NewPassword);

            if (result.Success)
            {
                App.NavigateToAndClearBackStack(typeof(LoginPage));
            }
            else
            {
                ErrorMessage = result.Error ?? "Unknown error occurred";
            }
        }



    }


}