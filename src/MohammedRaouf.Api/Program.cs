using MohammedRaouf.Api.Endpoints;
using MohammedRaouf.Api.Health;
using MohammedRaouf.Api.Middleware;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application;
using MohammedRaouf.Application.Identity;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Contracts.Health;
using MohammedRaouf.Infrastructure;
using MohammedRaouf.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
    builder.Services.Configure<VideoOptions>(builder.Configuration.GetSection(VideoOptions.SectionName));
    builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.SectionName));
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Extensions["correlationId"] = CorrelationIdMiddleware.Read(context.HttpContext);
        };
    });
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddOpenApi();
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
        .AddCheck<PostgresReadyHealthCheck>("postgres", tags: ["ready"]);
    builder.Services.AddHttpContextAccessor();
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 3L * 1024 * 1024 * 1024;
    });
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 3L * 1024 * 1024 * 1024;
    });
    builder.Services.AddScoped<AuthCookieWriter>();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPlatformAuthentication(builder.Configuration);
    builder.Services.AddPlatformRateLimiting(builder.Configuration);
    builder.Services.AddPlatformCors(builder.Configuration, builder.Environment);
    builder.Services.AddHsts(options =>
    {
        options.MaxAge = TimeSpan.FromDays(365);
        options.IncludeSubDomains = true;
        options.Preload = false;
    });
    builder.Services.AddHttpsRedirection(options =>
    {
        options.HttpsPort = 443;
        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    });
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        if (builder.Configuration.GetValue("ForwardedHeaders:AllowAllProxies", false))
        {
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        }
    });

    var app = builder.Build();
    ProductionHostValidator.Validate(app.Configuration, app.Environment);

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await dbContext.Database.CanConnectAsync())
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "VideoTranscodeJobs" (
                    "Id" uuid NOT NULL,
                    "LessonId" uuid NOT NULL,
                    "Status" character varying(32) NOT NULL,
                    "SourceRelativePath" character varying(500) NOT NULL,
                    "OutputRelativePath" character varying(500) NULL,
                    "ErrorMessage" character varying(2000) NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    "StartedAt" timestamp with time zone NULL,
                    "CompletedAt" timestamp with time zone NULL,
                    CONSTRAINT "PK_VideoTranscodeJobs" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_VideoTranscodeJobs_Lessons_LessonId" FOREIGN KEY ("LessonId") REFERENCES "Lessons" ("Id") ON DELETE CASCADE
                );
                CREATE INDEX IF NOT EXISTS "IX_VideoTranscodeJobs_LessonId" ON "VideoTranscodeJobs" ("LessonId");
                CREATE INDEX IF NOT EXISTS "IX_VideoTranscodeJobs_Status_CreatedAt" ON "VideoTranscodeJobs" ("Status", "CreatedAt");
                """);
            var seeder = scope.ServiceProvider.GetRequiredService<IIdentityDataSeeder>();
            await seeder.SeedAsync();
        }
    }

    app.UseForwardedHeaders();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseExceptionHandler();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
        if (!app.Configuration.GetValue("App:DisableHttpsRedirection", false))
        {
            app.UseHttpsRedirection();
        }
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnostic, http) =>
        {
            diagnostic.Set("CorrelationId", CorrelationIdMiddleware.Read(http));
        };
    });
    app.UseCors(PlatformCorsExtensions.PolicyName);
    app.UseMiddleware<CsrfProtectionMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "MohammedRaouf API v1");
        });
    }

    MapHealth(app);
    app.MapAuthEndpoints();
    app.MapProfileEndpoints();
    app.MapPublicEndpoints();
    app.MapCourseAccessEndpoints();
    app.MapAdminCourseEndpoints();
    app.MapMediaEndpoints();
    app.MapPurchaseRequestEndpoints();
    app.MapAdminPurchaseRequestEndpoints();
    app.MapActivationEndpoints();
    app.MapEnrollmentEndpoints();
    app.MapStudentLearningEndpoints();
    app.MapLessonProgressEndpoints();
    app.MapAdminDashboardEndpoints();
    app.MapAdminStudentEndpoints();
    app.MapAdminUserEndpoints();
    app.MapAdminAuditLogEndpoints();
    app.MapConsultationEndpoints();
    app.MapContactEndpoints();
    app.MapAdminCmsEndpoints();

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

static void MapHealth(WebApplication app)
{
    app.MapGet("/health", () => Results.Ok(new HealthResponse
        {
            Status = "Healthy",
            Service = "MohammedRaouf.Api",
            TimestampUtc = DateTimeOffset.UtcNow
        }))
        .WithName("Health")
        .WithTags("Health")
        .AllowAnonymous()
        .DisableRateLimiting();

    app.MapGet("/health/live", () => Results.Ok(new HealthResponse
        {
            Status = "Healthy",
            Service = "MohammedRaouf.Api",
            TimestampUtc = DateTimeOffset.UtcNow
        }))
        .WithName("HealthLive")
        .WithTags("Health")
        .AllowAnonymous()
        .DisableRateLimiting();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        })
        .AllowAnonymous()
        .DisableRateLimiting();
}

public partial class Program;
