using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OnlineLearningPlatform.Models.Entities.Identity;

namespace OnlineLearningPlatform.Models.Migrations.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // ===== SEED ROLES =====
            string[] roles = { "Admin", "Student", "Teacher" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (!result.Succeeded)
                        throw new Exception($"Failed to create role {role}");
                }
            }

            // ===== SEED ADMIN =====
            await CreateUserIfNotExists(userManager,
                "admin@gmail.com",
                "Admin@123", "Admin",
                "Admin");

            // ===== SEED TEACHER =====
            await CreateUserIfNotExists(userManager,
                "teacher@gmail.com",
                "Teacher@123", "Teacher",
                "Teacher");

            // ===== SEED STUDENT =====
            await CreateUserIfNotExists(userManager,
                "student@gmail.com",
                "Student@123", "Student",
                "Student");
        }

        private static async Task CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
                return;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName =  fullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
                throw new Exception($"Failed to create user {email}");

            await userManager.AddToRoleAsync(user, role);
        }
    }
}