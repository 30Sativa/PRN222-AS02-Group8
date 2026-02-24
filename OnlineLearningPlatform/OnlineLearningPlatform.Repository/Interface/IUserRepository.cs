using Microsoft.AspNetCore.Identity;
using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IUserRepository
    {
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<SignInResult> PasswordSignInAsync(string email, string password);
        Task AddToRoleAsync(ApplicationUser user, string role);
        Task SignOutAsync();

    }
}
