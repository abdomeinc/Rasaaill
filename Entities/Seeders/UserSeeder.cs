using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Entities.Seeders
{
    public static class UserSeeder
    {
        private const string SharedPassword = "P@$$w0rd";
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Models.User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // Add predefined users
            var predefinedUsers = new List<(string Email, string Password, string DisplayName, string Role)>
            {
                ("a.masry@misrtech-eg.com", SharedPassword, "Ali Al-Masry", "Admin"),
                ("mabdelghany@misrtech-eg.com", SharedPassword, "Mona Abdel Ghany", "User"),
                ("morfy@misrtech-eg.com", SharedPassword, "Moyasser Orfy", "User"),
                ("kaziz@misrtech-eg.com", SharedPassword, "Kamal Abdel Aziz", "User"),
                ("omaima@misrtech-eg.com", SharedPassword, "Omaima Samir", "User"),
                ("mghaly@misrtech-eg.com", SharedPassword, "Mohammad Ghaly", "User"),
                ("masood@misrtech-eg.com", SharedPassword, "Masood Farag", "User"),
                ("nour@misrtech-eg.com", SharedPassword, "Nour Habib", "User"),
            };

            // 1. Ensure roles exist
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }

            foreach (var (email, password, displayName, role) in predefinedUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    var user = new Models.User
                    {
                        UserName = email,
                        Email = email,
                        DisplayName = displayName,
                        AvatarUrl = "/media/defaults/avatars/user-avatar.png",
                        EmailConfirmed = true,
                        CreationDate = DateTime.UtcNow,
                        IsApproved = true
                    };

                    var result = await userManager.CreateAsync(user, password);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new Exception($"Failed to create user {email}: {errors}");
                    }

                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
