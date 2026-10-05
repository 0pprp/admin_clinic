using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MohammedRaouf.Application.Courses;

namespace MohammedRaouf.IntegrationTests;

public class ProductionApiFactory : AuthApiFactory
{
    protected override string EnvironmentName => "Production";

    protected virtual bool DisableHttpsRedirection => true;

    protected override Dictionary<string, string?> ExtraSettings() => new()
    {
        ["Jwt:SigningKey"] = "prod-test-signing-key-do-not-ship!!",
        ["PublicAppUrl"] = "https://example.com",
        ["App:PublicUrl"] = "https://example.com",
        ["App:AllowLocalDatabase"] = "true",
        ["App:DisableHttpsRedirection"] = DisableHttpsRedirection ? "true" : "false",
        ["Cors:AllowedOrigins:0"] = "https://example.com",
        ["Email:Provider"] = "Disabled",
        ["Video:Provider"] = "Unavailable",
        ["ForwardedHeaders:AllowAllProxies"] = "false"
    };
}

public sealed class HttpsRedirectProductionApiFactory : ProductionApiFactory
{
    protected override bool DisableHttpsRedirection => false;
}

public sealed class VideoCaptureApiFactory : AuthApiFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services.Where(item => item.ServiceType == typeof(IVideoPlaybackService)).ToList())
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<CapturingVideoPlaybackService>();
            services.AddSingleton<IVideoPlaybackService>(provider =>
                provider.GetRequiredService<CapturingVideoPlaybackService>());
        });
    }
}

public sealed class CapturingVideoPlaybackService : IVideoPlaybackService
{
    private readonly List<Guid> _lessonIds = [];

    public IReadOnlyList<Guid> LessonIds
    {
        get
        {
            lock (_lessonIds)
            {
                return [.. _lessonIds];
            }
        }
    }

    public void Reset()
    {
        lock (_lessonIds)
        {
            _lessonIds.Clear();
        }
    }

    public Task<VideoPlaybackResult?> GetPlaybackAsync(Guid lessonId, CancellationToken cancellationToken = default)
    {
        lock (_lessonIds)
        {
            _lessonIds.Add(lessonId);
        }

        return Task.FromResult<VideoPlaybackResult?>(new VideoPlaybackResult
        {
            PlaybackUnavailable = false,
            Message = "جاهز للتشغيل.",
            PlaybackUrl = "https://iframe.mediadelivery.net/embed/test/video",
            Provider = "BunnyStream",
            Kind = "iframe"
        });
    }
}

public sealed class BunnyPlaybackApiFactory : AuthApiFactory
{
    protected override Dictionary<string, string?> ExtraSettings()
    {
        var settings = base.ExtraSettings();
        settings["Video:Provider"] = "BunnyStream";
        settings["Video:Bunny:LibraryId"] = "12345";
        settings["Video:Bunny:TokenKey"] = "unit-test-bunny-token-key";
        settings["Video:Bunny:CdnHostname"] = "iframe.mediadelivery.net";
        settings["Video:TokenLifetimeMinutes"] = "10";
        return settings;
    }
}
