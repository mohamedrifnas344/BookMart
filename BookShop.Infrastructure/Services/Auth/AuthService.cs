using BookShop.Application.IServices;
using BookShop.Application.ViewModels.Account;
using BookShop.Application.ViewModels.UserManagement;
using BookShop.Domain.Entities;
using BookShop.Utility.IdentityHelper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookShop.Infrastructure.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthService(UserManager<AppUser> userManager ,
                           SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<int> CountAsync()
        {
            return await _userManager.Users
                .Where(a => a.Email != Roles.AdminEmail).CountAsync();
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return false;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<IEnumerable<UserResponseVM>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .Where(a => a.Email != Roles.AdminEmail).ToListAsync();
            var usersListVM = new List<UserResponseVM>();
            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersListVM.Add(new UserResponseVM
                {
                    UserId = user.Id,
                    FirstName = user.FirstName ?? "N/A",
                    LastName = user.LastName ?? "N/A",
                    Email = user.Email ?? "N/A",
                    Role = roles.FirstOrDefault() ?? "N/A"
                });
            }

            return usersListVM;
        }

        public async Task<AuthResult> Login(LoginRequestVM requestVM)
        {
            var user = await _userManager.FindByEmailAsync(requestVM.Email);
            if(user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Email or password is incorrect. Please check and try again."
                };
            };

            var userResult = await _signInManager.PasswordSignInAsync(user, requestVM.Password,
                                                                      isPersistent: true, lockoutOnFailure: false);
            if (userResult.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                return new AuthResult
                {
                    Success = true,
                    Message = "Login successful",
                    Roles = roles
                };
            }

            return new AuthResult
            {
                Success = false,
                Message = "Email or password is incorrect. Please check and try again."
            };
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<AuthResult> Register(RegisterRequestVM requestVM)
        {
            var user = await _userManager.FindByEmailAsync(requestVM.Email);
            if(user != null)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Email is already in use."
                };
            }

            var identityUser = new AppUser
            {
                FirstName = requestVM.FirstName,
                LastName = requestVM.LastName,
                Email = requestVM.Email,
                UserName = requestVM.Email
            };

            var identityResult = await _userManager.CreateAsync(identityUser, requestVM.Password);
            if (!identityResult.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Registration failed."
                };
            }

            var roleResult = await _userManager.AddToRoleAsync(identityUser, Roles.User);
            if (!roleResult.Succeeded)
            {
                return new AuthResult
                {
                    Success = false,
                    Message = "Role assignment failed."
                };
            }

            return new AuthResult
            {
                Success = true,
                Message = "Registration completed successfully."
            };
        }

        public async Task<bool> UpdateUserRoleAsync(UpdateUserRoleVM roleVM)
        {
            var user = await _userManager.FindByIdAsync(roleVM.UserId);
            if(user == null)
            {
                return false;
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var roleResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!roleResult.Succeeded)
                {
                    return false;
                }
            }

            var newRoleResult = await _userManager.AddToRoleAsync(user, roleVM.Role);
            return newRoleResult.Succeeded;
        }

        public async Task<UserResponseVM> UserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponseVM
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? "N/A",
                LastName = user.LastName ?? "N/A",
                Email = user.Email ?? "N/A",
                Role = roles.FirstOrDefault() ?? "N/A"
            };
        }
    }
}
