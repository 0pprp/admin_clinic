using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MohammedRaouf.Application.Security;
using MohammedRaouf.Domain.Entities;
using MohammedRaouf.Domain.Enums;
using MohammedRaouf.Infrastructure.Persistence;

namespace MohammedRaouf.Infrastructure.Courses;

public sealed class SelfHostedVideoService(
    ApplicationDbContext dbContext,
    IOptions<VideoOptions> options,
    TimeProvider timeProvider,
    ILogger<SelfHostedVideoService> logger) : Application.Courses.ISelfHostedVideoService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".mkv", ".webm", ".m4v"
    };

    public async Task<Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>> GetStatusAsync(
        Guid lessonId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons.AsNoTracking().FirstOrDefaultAsync(item => item.Id == lessonId, cancellationToken);
        if (lesson is null)
        {
            return Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>.Fail(404, "غير موجود", "الدرس غير موجود.");
        }

        return Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>.Ok(await MapStatusAsync(lesson, cancellationToken));
    }

    public async Task<Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>> UploadAsync(
        Guid lessonId,
        Stream content,
        string fileName,
        long contentLength,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        var lesson = await dbContext.Lessons.FirstOrDefaultAsync(item => item.Id == lessonId, cancellationToken);
        if (lesson is null)
        {
            return Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>.Fail(404, "غير موجود", "الدرس غير موجود.");
        }

        var settings = options.Value.SelfHosted;
        if (string.IsNullOrWhiteSpace(settings.SigningKey))
        {
            return Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>.Fail(
                503, "غير مهيأ", "مفتاح توقيع الفيديو الذاتي غير مضبوط على السيرفر.");
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>.Fail(
                400, "صيغة غير مدعومة", "ارفع ملف فيديو بصيغة mp4 أو mov أو mkv أو webm.");
        }

        var maxBytes = Math.Max(1, settings.MaxUploadMegabytes) * 1024L * 1024L;
        if (contentLength > maxBytes)
        {
            return Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>.Fail(
                400, "حجم كبير", $"الحد الأقصى للرفع هو {settings.MaxUploadMegabytes} ميغابايت.");
        }

        var root = Path.GetFullPath(settings.RootPath);
        Directory.CreateDirectory(root);
        var lessonFolder = Path.Combine(root, "lessons", lessonId.ToString("N"));
        Directory.CreateDirectory(lessonFolder);
        var sourceRelative = Path.Combine("lessons", lessonId.ToString("N"), $"source{extension}").Replace('\\', '/');
        var sourceFull = Path.Combine(root, sourceRelative.Replace('/', Path.DirectorySeparatorChar));

        await using (var output = File.Create(sourceFull))
        {
            await content.CopyToAsync(output, cancellationToken);
        }

        var now = timeProvider.GetUtcNow();
        var existing = await dbContext.VideoTranscodeJobs
            .Where(item => item.LessonId == lessonId &&
                           (item.Status == VideoTranscodeStatus.Queued || item.Status == VideoTranscodeStatus.Processing))
            .ToListAsync(cancellationToken);
        foreach (var job in existing)
        {
            job.Status = VideoTranscodeStatus.Failed;
            job.ErrorMessage = "تم استبداله برفع أحدث.";
            job.UpdatedAt = now;
            job.CompletedAt = now;
        }

        var created = new VideoTranscodeJob
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            Status = VideoTranscodeStatus.Queued,
            SourceRelativePath = sourceRelative,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.VideoTranscodeJobs.Add(created);

        lesson.VideoProvider = VideoProvider.SelfHostedHls;
        lesson.VideoKey = null;
        lesson.UpdatedAt = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Queued self-hosted video upload for lesson {LessonId} by {Actor}.", lessonId, actorUserId);
        return Application.Common.ActionResult<Contracts.Admin.AdminLessonVideoStatusResponse>.Ok(await MapStatusAsync(lesson, cancellationToken));
    }

    private async Task<Contracts.Admin.AdminLessonVideoStatusResponse> MapStatusAsync(Lesson lesson, CancellationToken cancellationToken)
    {
        var job = await dbContext.VideoTranscodeJobs.AsNoTracking()
            .Where(item => item.LessonId == lesson.Id)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var status = job?.Status.ToString() ??
                     (string.IsNullOrWhiteSpace(lesson.VideoKey) ? "None" : "Ready");
        var message = job?.Status switch
        {
            VideoTranscodeStatus.Queued => "بانتظار المعالجة...",
            VideoTranscodeStatus.Processing => "جاري تحويل الفيديو لجودة متعددة (مثل يوتيوب)...",
            VideoTranscodeStatus.Ready => "الفيديو جاهز للتشغيل بجودة تلقائية.",
            VideoTranscodeStatus.Failed => job.ErrorMessage ?? "فشلت المعالجة.",
            _ => string.IsNullOrWhiteSpace(lesson.VideoKey) ? "لا يوجد فيديو مرفوع." : "الفيديو جاهز."
        };

        return new Contracts.Admin.AdminLessonVideoStatusResponse
        {
            LessonId = lesson.Id,
            VideoProvider = lesson.VideoProvider.ToString(),
            VideoKey = lesson.VideoKey,
            ProcessingStatus = status,
            Message = message,
            DurationSeconds = lesson.DurationSeconds
        };
    }
}

public sealed class VideoTranscodeWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<VideoOptions> options,
    ILogger<VideoTranscodeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Video transcode worker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNextAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Video transcode worker loop failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessNextAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var time = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var root = Path.GetFullPath(options.Value.SelfHosted.RootPath);

        var job = await db.VideoTranscodeJobs
            .Where(item => item.Status == VideoTranscodeStatus.Queued)
            .OrderBy(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (job is null)
        {
            return;
        }

        var lesson = await db.Lessons.FirstOrDefaultAsync(item => item.Id == job.LessonId, cancellationToken);
        if (lesson is null)
        {
            job.Status = VideoTranscodeStatus.Failed;
            job.ErrorMessage = "الدرس غير موجود.";
            job.UpdatedAt = time.GetUtcNow();
            job.CompletedAt = job.UpdatedAt;
            await db.SaveChangesAsync(cancellationToken);
            return;
        }

        job.Status = VideoTranscodeStatus.Processing;
        job.StartedAt = time.GetUtcNow();
        job.UpdatedAt = job.StartedAt.Value;
        await db.SaveChangesAsync(cancellationToken);

        var sourceFull = Path.Combine(root, job.SourceRelativePath.Replace('/', Path.DirectorySeparatorChar));
        var outputDir = Path.Combine(root, "lessons", lesson.Id.ToString("N"), "hls");
        try
        {
            if (!File.Exists(sourceFull))
            {
                throw new InvalidOperationException("ملف المصدر غير موجود.");
            }

            if (Directory.Exists(outputDir))
            {
                Directory.Delete(outputDir, recursive: true);
            }

            Directory.CreateDirectory(outputDir);
            await RunFfmpegAsync(sourceFull, outputDir, cancellationToken);

            var master = Path.Combine(outputDir, "master.m3u8");
            if (!File.Exists(master))
            {
                throw new InvalidOperationException("لم يُنشأ ملف master.m3u8.");
            }

            var duration = await ProbeDurationSecondsAsync(sourceFull, cancellationToken);
            var relative = Path.Combine("lessons", lesson.Id.ToString("N"), "hls", "master.m3u8").Replace('\\', '/');
            lesson.VideoProvider = VideoProvider.SelfHostedHls;
            lesson.VideoKey = relative;
            if (duration > 0)
            {
                lesson.DurationSeconds = duration;
            }

            lesson.UpdatedAt = time.GetUtcNow();
            job.OutputRelativePath = relative;
            job.Status = VideoTranscodeStatus.Ready;
            job.ErrorMessage = null;
            job.CompletedAt = lesson.UpdatedAt;
            job.UpdatedAt = lesson.UpdatedAt;
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Transcoded lesson {LessonId} to HLS.", lesson.Id);
        }
        catch (Exception ex)
        {
            job.Status = VideoTranscodeStatus.Failed;
            job.ErrorMessage = ex.Message.Length > 1900 ? ex.Message[..1900] : ex.Message;
            job.CompletedAt = time.GetUtcNow();
            job.UpdatedAt = job.CompletedAt.Value;
            await db.SaveChangesAsync(cancellationToken);
            logger.LogError(ex, "Failed transcoding lesson {LessonId}.", job.LessonId);
        }
    }

    private static async Task RunFfmpegAsync(string sourceFull, string outputDir, CancellationToken cancellationToken)
    {
        // Adaptive ladder: 360p / 720p / 1080p — player picks by bandwidth (YouTube-like).
        var args =
            $"-y -i \"{sourceFull}\" " +
            "-filter_complex \"[0:v]split=3[v1][v2][v3];[v1]scale=w=640:h=360:force_original_aspect_ratio=decrease,pad=640:360:(ow-iw)/2:(oh-ih)/2[v1out];[v2]scale=w=1280:h=720:force_original_aspect_ratio=decrease,pad=1280:720:(ow-iw)/2:(oh-ih)/2[v2out];[v3]scale=w=1920:h=1080:force_original_aspect_ratio=decrease,pad=1920:1080:(ow-iw)/2:(oh-ih)/2[v3out]\" " +
            "-map \"[v1out]\" -map a:0? -c:v:0 libx264 -b:v:0 800k -c:a:0 aac -b:a:0 96k -ac:a:0 2 " +
            "-map \"[v2out]\" -map a:0? -c:v:1 libx264 -b:v:1 2500k -c:a:1 aac -b:a:1 128k -ac:a:1 2 " +
            "-map \"[v3out]\" -map a:0? -c:v:2 libx264 -b:v:2 5000k -c:a:2 aac -b:a:2 192k -ac:a:2 2 " +
            "-f hls -hls_time 4 -hls_playlist_type vod -hls_flags independent_segments " +
            "-master_pl_name master.m3u8 " +
            "-var_stream_map \"v:0,a:0 v:1,a:1 v:2,a:2\" " +
            $"-hls_segment_filename \"{Path.Combine(outputDir, "stream_%v", "seg_%03d.ts").Replace('\\', '/')}\" " +
            $"\"{Path.Combine(outputDir, "stream_%v", "prog.m3u8").Replace('\\', '/')}\"";

        Directory.CreateDirectory(Path.Combine(outputDir, "stream_0"));
        Directory.CreateDirectory(Path.Combine(outputDir, "stream_1"));
        Directory.CreateDirectory(Path.Combine(outputDir, "stream_2"));

        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = args,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("تعذر تشغيل ffmpeg. تأكد من تثبيته على السيرفر.");
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var stderr = await stderrTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"ffmpeg failed ({process.ExitCode}): {Trim(stderr)}");
        }
    }

    private static async Task<int> ProbeDurationSecondsAsync(string sourceFull, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{sourceFull}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process is null)
            {
                return 0;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            return double.TryParse(output.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds)
                ? (int)Math.Round(seconds)
                : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static string Trim(string value) =>
        value.Length <= 800 ? value : value[^800..];
}
