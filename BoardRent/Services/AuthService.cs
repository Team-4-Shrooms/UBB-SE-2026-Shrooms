using BoardRent.Domain;
using BoardRent.DTOs;
using BoardRent.Repositories;
using BoardRent.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BoardRent.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IFailedLoginRepository _failedLoginRepository;

        public AuthService(IUserRepository userRepository, IFailedLoginRepository failedLoginRepository)
        {
            _userRepository = userRepository;
            _failedLoginRepository = failedLoginRepository;
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDto dto)
        {
            var existingUsername = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUsername != null)
                return ServiceResult<bool>.Fail("Username is already taken.");

            var existingEmail = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingEmail != null)
                return ServiceResult<bool>.Fail("Email is already registered.");

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                DisplayName = dto.DisplayName,
                Username = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Country = dto.Country,
                City = dto.City,
                StreetName = dto.StreetName,
                StreetNumber = dto.StreetNumber,
                PasswordHash = PasswordHasher.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSuspended = false
            };

            await _userRepository.AddAsync(newUser);
            await _userRepository.AssignRoleAsync(newUser.Id, "Standard User");

            SessionContext.GetInstance().Populate(newUser, "Standard User");

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<UserProfileDto>> LoginAsync(LoginDto dto)
        {
            User user = await _userRepository.GetByUsernameAsync(dto.UsernameOrEmail);
            if (user == null)
            {
                user = await _userRepository.GetByEmailAsync(dto.UsernameOrEmail);
            }

            if (user == null)
            {
                return ServiceResult<UserProfileDto>.Fail("Sign-in was unsuccessful. Please check your credentials and try again.");
            }

            var failedLogin = await _failedLoginRepository.GetByUserIdAsync(user.Id);
            if (failedLogin != null && failedLogin.LockedUntil.HasValue && failedLogin.LockedUntil.Value > DateTime.UtcNow)
            {
                var remaining = failedLogin.LockedUntil.Value - DateTime.UtcNow;
                int minutes = (int)Math.Ceiling(remaining.TotalMinutes);
                return ServiceResult<UserProfileDto>.Fail($"Your account is locked due to too many failed sign-in attempts. Please try again in {minutes} minute{(minutes != 1 ? "s" : "")}.");
            }

            if (user.IsSuspended)
            {
                return ServiceResult<UserProfileDto>.Fail("This account has been suspended. Please contact support.");
            }

            if (!PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                await _failedLoginRepository.IncrementAsync(user.Id);
                return ServiceResult<UserProfileDto>.Fail("Sign-in was unsuccessful. Please check your credentials and try again.");
            }

            await _failedLoginRepository.ResetAsync(user.Id);

            string roleName = user.Roles?.FirstOrDefault()?.Name ?? "User";

            SessionContext.GetInstance().Populate(user, roleName);

            var profileDto = new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                AvatarUrl = user.AvatarUrl,
                Role = roleName,
                IsSuspended = user.IsSuspended,
                Country = user.Country,
                City = user.City,
                StreetName = user.StreetName,
                StreetNumber = user.StreetNumber
            };

            return ServiceResult<UserProfileDto>.Ok(profileDto);
        }

        public async Task<ServiceResult<bool>> LogoutAsync()
        {
            SessionContext.GetInstance().Clear();
            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<string>> ForgotPasswordAsync()
        {
            return ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com to reset your password.");
        }

    }
}
