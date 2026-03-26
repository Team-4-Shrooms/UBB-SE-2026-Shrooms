using BoardRent.DTOs;
using BoardRent.Services;
using BoardRent.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BoardRent.ViewModels
{
    public partial class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;

        [ObservableProperty] private string displayName = string.Empty;
        [ObservableProperty] private string username = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private string phoneNumber = string.Empty;
        [ObservableProperty] private string country = string.Empty;
        [ObservableProperty] private string city = string.Empty;
        [ObservableProperty] private string streetName = string.Empty;
        [ObservableProperty] private string streetNumber = string.Empty;

        [ObservableProperty] private string displayNameError = string.Empty;
        [ObservableProperty] private string usernameError = string.Empty;
        [ObservableProperty] private string emailError = string.Empty;
        [ObservableProperty] private string passwordError = string.Empty;
        [ObservableProperty] private string confirmPasswordError = string.Empty;
        [ObservableProperty] private string phoneNumberError = string.Empty;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            displayNameError = string.Empty;
            usernameError = string.Empty;
            emailError = string.Empty;
            passwordError = string.Empty;
            confirmPasswordError = string.Empty;
            phoneNumberError = string.Empty;

            if (string.IsNullOrWhiteSpace(DisplayName))
            {
                displayNameError = "Display name is required.";
                isValid = false;
            }
            else if (DisplayName.Trim().Length < 2 || DisplayName.Trim().Length > 50)
            {
                displayNameError = "Display name must be between 2 and 50 characters.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Username))
            {
                usernameError = "Username is required.";
                isValid = false;
            }
            else if (Username.Trim().Length < 3 || Username.Trim().Length > 30)
            {
                usernameError = "Username must be between 3 and 30 characters.";
                isValid = false;
            }
            else if (!Regex.IsMatch(Username.Trim(), @"^[a-zA-Z0-9_]+$"))
            {
                usernameError = "Username may only contain letters, numbers, and underscores.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                emailError = "Email is required.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                passwordError = "Password is required.";
                isValid = false;
            }
            else if (Password.Length < 8)
            {
                passwordError = "Password must be at least 8 characters long.";
                isValid = false;
            }
            else if (!Regex.IsMatch(Password, @"[A-Z]"))
            {
                passwordError = "Password must contain at least one uppercase letter.";
                isValid = false;
            }
            else if (!Regex.IsMatch(Password, @"[0-9]"))
            {
                passwordError = "Password must contain at least one number.";
                isValid = false;
            }
            else if (!Regex.IsMatch(Password, @"[^a-zA-Z0-9]"))
            {
                passwordError = "Password must contain at least one special character.";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                confirmPasswordError = "Please confirm your password.";
                isValid = false;
            }
            else if (Password != ConfirmPassword)
            {
                confirmPasswordError = "Passwords do not match.";
                isValid = false;
            }

            if (!string.IsNullOrWhiteSpace(PhoneNumber) && !Regex.IsMatch(PhoneNumber.Trim(), @"^\+?[0-9\s\-()]{7,15}$"))
            {
                phoneNumberError = "Please enter a valid phone number.";
                isValid = false;
            }

            return isValid;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            ErrorMessage = string.Empty;

            if (!ValidateFields())
            {
                return;
            }

            IsLoading = true;

            var registerDto = new RegisterDto
            {
                DisplayName = this.DisplayName,
                Username = this.Username,
                Email = this.Email,
                Password = this.Password,
                ConfirmPassword = this.ConfirmPassword,
                PhoneNumber = this.PhoneNumber,
                Country = this.Country,
                City = this.City,
                StreetName = this.StreetName,
                StreetNumber = this.StreetNumber
            };

            var result = await _authService.RegisterAsync(registerDto);

            if (result.Success)
            {
                App.NavigateTo(typeof(ProfilePage));
            }
            else
            {
                var error = result.Error ?? "Registration failed.";
                if (error.Contains("Username"))
                {
                    usernameError = error;
                }
                else if (error.Contains("Email"))
                {
                    emailError = error;
                }
                else
                {
                    ErrorMessage = error;
                }
            }

            IsLoading = false;
        }

        [RelayCommand]
        private void GoToLogin()
        {
            App.NavigateBack();
        }
    }
}
