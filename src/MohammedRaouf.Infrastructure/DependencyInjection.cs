using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Application.Admin;
using MohammedRaouf.Application.Auth;
using MohammedRaouf.Application.Catalog;
using MohammedRaouf.Application.Cms;
using MohammedRaouf.Application.Consultations;
using MohammedRaouf.Application.Contact;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Application.Identity;
using MohammedRaouf.Application.Notifications;
using MohammedRaouf.Application.Activation;
using MohammedRaouf.Application.Purchases;
using MohammedRaouf.Domain.Identity;
using MohammedRaouf.Infrastructure.Activation;
using MohammedRaouf.Infrastructure.Admin;
using MohammedRaouf.Infrastructure.Auth;
using MohammedRaouf.Infrastructure.Catalog;
using MohammedRaouf.Infrastructure.Cms;
using MohammedRaouf.Infrastructure.Consultations;
using MohammedRaouf.Infrastructure.Contact;
using MohammedRaouf.Infrastructure.Courses;
using MohammedRaouf.Infrastructure.Email;
using MohammedRaouf.Infrastructure.Identity;
using MohammedRaouf.Infrastructure.Persistence;
using MohammedRaouf.Infrastructure.Purchases;
using MohammedRaouf.Application.Learning;
using MohammedRaouf.Infrastructure.Learning;

namespace MohammedRaouf.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var resolvedConfiguration = serviceProvider.GetRequiredService<IConfiguration>();
            var resolvedConnectionString = resolvedConfiguration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(resolvedConnectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            options.UseNpgsql(resolvedConnectionString);
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        services.AddSingleton(TimeProvider.System);
        services.Configure<MohammedRaouf.Application.Security.JwtOptions>(
            configuration.GetSection(MohammedRaouf.Application.Security.JwtOptions.SectionName));
        services.Configure<MohammedRaouf.Application.Security.VideoOptions>(
            configuration.GetSection(MohammedRaouf.Application.Security.VideoOptions.SectionName));
        services.Configure<ActivationCodeOptions>(
            configuration.GetSection(ActivationCodeOptions.SectionName));
        services.Configure<MohammedRaouf.Application.Security.EmailOptions>(
            configuration.GetSection(MohammedRaouf.Application.Security.EmailOptions.SectionName));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityDataSeeder, IdentityDataSeeder>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IEmailOtpService, EmailOtpService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPublicCatalogService, PublicCatalogService>();
        services.AddScoped<ICourseAccessService, CourseAccessService>();
        services.AddScoped<ICourseQueryService, CourseQueryService>();
        services.AddScoped<ICourseManagementService, CourseManagementService>();
        services.AddScoped<BunnyStreamPlaybackService>();
        services.AddScoped<SelfHostedHlsPlaybackService>();
        services.AddScoped<UnavailableVideoPlaybackService>();
        services.AddScoped<IVideoPlaybackService, CompositeVideoPlaybackService>();
        services.AddScoped<ISelfHostedVideoService, SelfHostedVideoService>();
        services.AddHostedService<VideoTranscodeWorker>();
        services.AddSingleton<IPurchaseRequestStateMachine, PurchaseRequestStateMachine>();
        services.AddScoped<IRequestNumberGenerator, RequestNumberGenerator>();
        services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
        services.AddScoped<IAdminPurchaseRequestService, AdminPurchaseRequestService>();
        services.AddScoped<IActivationWorkflowService, ActivationWorkflowService>();
        services.AddScoped<StudentLearningService>();
        services.AddScoped<IStudentLearningService>(provider => provider.GetRequiredService<StudentLearningService>());
        services.AddScoped<IEnrollmentQueryService>(provider => provider.GetRequiredService<StudentLearningService>());
        services.AddScoped<ILessonProgressService, LessonProgressService>();
        var emailProvider = configuration["Email:Provider"] ?? "Logging";
        if (string.Equals(emailProvider, "Smtp", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(emailProvider, "Gmail", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IEmailService, SmtpEmailService>();
        }
        else if (string.Equals(emailProvider, "Disabled", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IEmailService, DisabledEmailService>();
        }
        else
        {
            services.AddSingleton<IEmailService, LoggingEmailService>();
        }
        services.AddScoped<IAdminAuditService, AdminAuditService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IAdminStudentService, AdminStudentService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAuditLogQueryService, AuditLogQueryService>();
        services.AddSingleton<IConsultationStateMachine, ConsultationStateMachine>();
        services.AddScoped<IConsultationNumberGenerator, ConsultationNumberGenerator>();
        services.AddScoped<IConsultationService, ConsultationService>();
        services.AddScoped<IContactMessageService, ContactMessageService>();
        services.AddScoped<ICmsAdminService, CmsAdminService>();
        services.AddScoped<ISiteSettingsAdminService, SiteSettingsAdminService>();

        return services;
    }
}
