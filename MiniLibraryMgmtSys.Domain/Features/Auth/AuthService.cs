using Microsoft.EntityFrameworkCore;
using MiniLibraryMgmtSys.Database.AppDbContextModels;
using MiniLibraryMgmtSys.DTOs;
using MiniLibraryMgmtSys.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MiniLibraryMgmtSys.Domain.Features.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(AppDbContext db, IJwtTokenService jwtTokenService)
        {
            _db = db;
            _jwtTokenService = jwtTokenService;
        }

        private IQueryable<TblUser> ActiveUser =>
            _db.TblUsers
            .Where(u => !u.DeleteFlag);

        #region email existence check
        public async Task<bool> EmailExistsAsync(string email)
        {
            email = email.Trim().ToLower();
            return await _db.TblUsers.AnyAsync(u => u.Email == email && !u.DeleteFlag);
        }
        #endregion

        #region email validation
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        #endregion

        #region email and password validation
        public async Task<TblUser?> ValidateUserAsync(string email, string password)
        {
            email = email.Trim().ToLower();

            var user = await ActiveUser
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.IsActive);

            if (user == null)
                return null;

            if (!PasswordHasher.Verify(password, user.Password))
                return null;

            return user;
        }
        #endregion

        #region user registration
        public async Task<ApiResponse<UserResponseDTO>> RegisterAsync(RegisterDTO request)
        {
            var newEmail = request.Email.Trim().ToLower();
            if (await EmailExistsAsync(newEmail))
                return ApiResponse<UserResponseDTO>.Failure("Email already registered.");

            if (!IsValidEmail(newEmail)) return ApiResponse<UserResponseDTO>.Failure("Invalid email format.");

            var existingUser = await _db.TblUsers
                .AnyAsync(u => u.Email == request.Email && !u.DeleteFlag);

            if (existingUser) return ApiResponse<UserResponseDTO>.Failure("User with this email already exists.");

            string hashedPassword = PasswordHasher.Hash(request.Password);

            var newUser = new TblUser
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                Password = hashedPassword,
                Role = request.Role ?? "Member",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                DeleteFlag = false
            };

            try
            {
                _db.TblUsers.Add(newUser);
                await _db.SaveChangesAsync();

                return ApiResponse<UserResponseDTO>.Success(new UserResponseDTO { Id = newUser.Id }, "User registered successfully.");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponseDTO>.Failure($"An error occurred during registration: {ex.Message}");
            }
        }
        #endregion

        #region login
        public async Task<ApiResponse<TokenResponse>> LoginAsync(LoginDTO request)
        {
            var user = await ValidateUserAsync(request.Email, request.Password);

            if (user == null)
            {
                return ApiResponse<TokenResponse>.Failure("Invalid credentials.");
            }

            var token = _jwtTokenService.GenerateAccessToken(user);

            var response = new TokenResponse
            {
                AccessToken = token,
                Email = user.Email,
                Role = user.Role
            };

            return ApiResponse<TokenResponse>.Success(response, "Login successful.");
        }
        #endregion
    }
}
