using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using MohammedRaouf.Api.Security;
using MohammedRaouf.Application.Authorization;
using MohammedRaouf.Application.Courses;
using MohammedRaouf.Application.Security;
using System.Security.Claims;

namespace MohammedRaouf.Api.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("/api/admin")
            .WithTags("AdminVideo")
            .RequireAuthorization(AuthorizationPolicies.ManageCourses);

        admin.MapGet("/lessons/{lessonId:guid}/video", GetStatusAsync);
        admin.MapPost("/lessons/{lessonId:guid}/video", UploadAsync)
            .DisableAntiforgery()
            .WithRequestTimeout(TimeSpan.FromHours(2));

        endpoints.MapGet("/api/media/v/{token}/lessons/{lessonId:guid}/{**assetPath}", ServeAsync)
            .WithTags("Media")
            .AllowAnonymous();

        return endpoints;
    }

    private static async Task<IResult> GetStatusAsync(
        Guid lessonId,
        ISelfHostedVideoService videos,
        CancellationToken cancellationToken)
    {
        var result = await videos.GetStatusAsync(lessonId, cancellationToken);
        return result.Succeeded
            ? Results.Ok(result.Value)
            : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
    }

    private static async Task<IResult> UploadAsync(
        Guid lessonId,
        HttpRequest request,
        ClaimsPrincipal user,
        ISelfHostedVideoService videos,
        CancellationToken cancellationToken)
    {
        var userId = user.GetUserId();
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        string fileName;
        Stream stream;
        long length;

        if (request.HasFormContentType)
        {
            var form = await request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("file");
            if (file is null || file.Length <= 0)
            {
                return Results.BadRequest(new
                {
                    title = "مطلوب ملف",
                    detail = "اختر ملف فيديو للرفع في الحقل file."
                });
            }

            fileName = file.FileName;
            length = file.Length;
            stream = file.OpenReadStream();
        }
        else
        {
            // Recover uploads that were incorrectly sent as application/json (old client bug).
            var recovered = await TryRecoverMultipartFileAsync(request, cancellationToken);
            if (recovered is null)
            {
                return Results.BadRequest(new
                {
                    title = "صيغة الطلب خاطئة",
                    detail = "يجب رفع الفيديو كـ multipart/form-data. حدّث الصفحة (Ctrl+F5) ثم أعد اختيار الملف والرفع."
                });
            }

            fileName = recovered.Value.FileName;
            length = recovered.Value.Length;
            stream = recovered.Value.Stream;
        }

        await using (stream)
        {
            var result = await videos.UploadAsync(lessonId, stream, fileName, length, userId.Value, cancellationToken);
            return result.Succeeded
                ? Results.Ok(result.Value)
                : Results.Problem(statusCode: result.StatusCode, title: result.Title, detail: result.Detail);
        }
    }

    private static async Task<(string FileName, long Length, Stream Stream)?> TryRecoverMultipartFileAsync(
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ContentLength is null or <= 0)
        {
            return null;
        }

        request.EnableBuffering();
        var memory = new MemoryStream();
        await request.Body.CopyToAsync(memory, cancellationToken);
        memory.Position = 0;

        var probe = new byte[Math.Min(128, (int)memory.Length)];
        _ = memory.Read(probe, 0, probe.Length);
        memory.Position = 0;
        var preamble = Encoding.UTF8.GetString(probe);
        if (!preamble.StartsWith("--", StringComparison.Ordinal))
        {
            return null;
        }

        var firstLine = preamble.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        var boundary = firstLine.TrimStart('-');
        if (string.IsNullOrWhiteSpace(boundary))
        {
            return null;
        }

        var reader = new MultipartReader(boundary, memory);
        while (await reader.ReadNextSectionAsync(cancellationToken) is { } section)
        {
            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition) ||
                !disposition.DispositionType.Equals("form-data") ||
                !disposition.Name.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fileName = disposition.FileName.HasValue
                ? HeaderUtilities.RemoveQuotes(disposition.FileName).Value ?? "upload.mp4"
                : "upload.mp4";
            var fileStream = new MemoryStream();
            await section.Body.CopyToAsync(fileStream, cancellationToken);
            fileStream.Position = 0;
            if (fileStream.Length <= 0)
            {
                await fileStream.DisposeAsync();
                return null;
            }

            return (fileName, fileStream.Length, fileStream);
        }

        return null;
    }

    private static async Task<IResult> ServeAsync(
        string token,
        Guid lessonId,
        string assetPath,
        IOptions<VideoOptions> options,
        CancellationToken cancellationToken)
    {
        var settings = options.Value.SelfHosted;
        if (!SelfHostedMediaToken.TryValidate(token, settings.SigningKey, out var tokenLessonId, out _) ||
            tokenLessonId != lessonId)
        {
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(assetPath) ||
            assetPath.Contains("..", StringComparison.Ordinal) ||
            Path.IsPathRooted(assetPath))
        {
            return Results.BadRequest();
        }

        var root = Path.GetFullPath(settings.RootPath);
        var full = Path.GetFullPath(Path.Combine(root, "lessons", lessonId.ToString("N"), "hls", assetPath.Replace('/', Path.DirectorySeparatorChar)));
        var allowedRoot = Path.GetFullPath(Path.Combine(root, "lessons", lessonId.ToString("N"), "hls"));
        if (!full.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
        {
            return Results.NotFound();
        }

        var contentType = full.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)
            ? "application/vnd.apple.mpegurl"
            : full.EndsWith(".ts", StringComparison.OrdinalIgnoreCase)
                ? "video/mp2t"
                : "application/octet-stream";

        var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Results.File(stream, contentType, enableRangeProcessing: true);
    }
}
