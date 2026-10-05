using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MohammedRaouf.Application.Identity;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Domain.Identity;

namespace MohammedRaouf.Infrastructure.Identity;

public sealed class IdentityDataSeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<IdentityDataSeeder> logger) : IIdentityDataSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var roleName in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var createRoleResult = await roleManager.CreateAsync(new IdentityRole<Guid>
                {
                    Id = Guid.NewGuid(),
                    Name = roleName
                });
                if (!createRoleResult.Succeeded)
                {
                    logger.LogError("Failed to seed role {RoleName}", roleName);
                }
            }
        }

        var adminEmail = configuration["SEED_ADMIN_EMAIL"];
        var adminPassword = configuration["SEED_ADMIN_PASSWORD"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogInformation("Admin seed skipped because SEED_ADMIN_EMAIL or SEED_ADMIN_PASSWORD is not set.");
            return;
        }

        var allowNonDevelopment = configuration.GetValue("SEED_ADMIN_ALLOW_NON_DEVELOPMENT", false);
        if (!environment.IsDevelopment() && !allowNonDevelopment)
        {
            logger.LogWarning("Admin seed refused because the environment is not Development.");
            return;
        }

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
        {
            return;
        }

        var utcNow = DateTimeOffset.UtcNow;
        var admin = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FullName = "Administrator",
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            AccountStatus = AccountStatus.Active,
            CreatedAt = utcNow,
            UpdatedAt = utcNow
        };

        var createUserResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createUserResult.Succeeded)
        {
            logger.LogError("Failed to seed development admin user.");
            return;
        }

        await userManager.AddToRoleAsync(admin, RoleNames.Admin);
        logger.LogInformation("Admin user was created from environment configuration.");
    }
}
