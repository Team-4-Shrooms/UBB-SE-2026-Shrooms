using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BoardRent.DTOs;
using BoardRent.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BoardRent.ViewModels
{
    public partial class AdminViewModel : BaseViewModel
    {
        private readonly IAdminService _adminService;
        private const int PageSize = 10;

        [ObservableProperty]
        private ObservableCollection<UserProfileDto> users = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnsuspendUserCommand))]
        [NotifyCanExecuteChangedFor(nameof(ResetPasswordCommand))]
        [NotifyCanExecuteChangedFor(nameof(UnlockAccountCommand))]
        private UserProfileDto selectedUser;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(PreviousPageCommand))]
        private int currentPage = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(NextPageCommand))]
        private int totalPages = 1;

        [ObservableProperty]
        private string successMessage = string.Empty;

        public AdminViewModel(IAdminService adminService)
        {
            _adminService = adminService;
            LoadUsersAsync().FireAndForgetSafeAsync();
        }

        public async Task LoadUsersAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var result = await _adminService.GetAllUsersAsync(CurrentPage, PageSize);
            if (result.Success && result.Data != null)
            {
                Users = new ObservableCollection<UserProfileDto>(result.Data);
                TotalPages = result.Data.Count == PageSize ? CurrentPage + 1 : CurrentPage;
            }
            else
            {
                ErrorMessage = result.Error ?? "Failed to load users.";
            }

            IsLoading = false;
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task SuspendUserAsync()
        {
            if (SelectedUser == null) return;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var result = await _adminService.SuspendUserAsync(SelectedUser.Id);
            if (result.Success)
            {
                SuccessMessage = $"User {SelectedUser.Username} has been suspended.";
                await LoadUsersAsync();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnsuspendUserAsync()
        {
            if (SelectedUser == null) return;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var result = await _adminService.UnsuspendUserAsync(SelectedUser.Id);
            if (result.Success)
            {
                SuccessMessage = $"User {SelectedUser.Username} has been unsuspended.";
                await LoadUsersAsync();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task ResetPasswordAsync()
        {
            if (SelectedUser == null) return;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var tempPassword = "Reset@1234";
            var result = await _adminService.ResetPasswordAsync(SelectedUser.Id, tempPassword);
            if (result.Success)
            {
                SuccessMessage = $"Password for {SelectedUser.Username} has been reset. Temporary password: {tempPassword}";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanModifySelectedUser))]
        private async Task UnlockAccountAsync()
        {
            if (SelectedUser == null) return;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            var result = await _adminService.UnlockAccountAsync(SelectedUser.Id);
            if (result.Success)
            {
                SuccessMessage = $"Account {SelectedUser.Username} has been unlocked.";
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadUsersAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadUsersAsync();
            }
        }

        private bool CanModifySelectedUser() => SelectedUser != null;
        private bool CanGoToPreviousPage() => CurrentPage > 1;
        private bool CanGoToNextPage() => CurrentPage < TotalPages;
    }

    public static class TaskExtensions
    {
        public static async void FireAndForgetSafeAsync(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                // Handle or log exception
            }
        }
    }
}
