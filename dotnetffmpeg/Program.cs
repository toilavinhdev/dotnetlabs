using System.Runtime.InteropServices;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.Lifetime.ApplicationStarted.Register(() =>
{
    var executablesPath = Path.Combine(builder.Environment.ContentRootPath, "FFmpeg");
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) executablesPath = Path.Combine(executablesPath, "windows");
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) executablesPath = Path.Combine(executablesPath, "linux");
    else throw new PlatformNotSupportedException();
    if (!File.Exists(Path.Combine(executablesPath, "ffmpeg.exe")) && !File.Exists(Path.Combine(executablesPath, "ffmpeg")))
    {
        app.Logger.LogInformation("Starting download FFmpeg: OSPlatform = {OS}", RuntimeInformation.OSDescription);
        FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, executablesPath).GetAwaiter().GetResult();
        var information = File.ReadAllText(Path.Combine(executablesPath, "version.json"));
        app.Logger.LogInformation("Finishing download FFmpeg: Information = {@Information}", information);
    }
    FFmpeg.SetExecutablesPath(executablesPath);
    app.Logger.LogInformation("FFmpeg executables path: {Path}", executablesPath);
});
app.UseSwagger();
app.UseSwaggerUI();
app.Map("/", () => "Hello World!");
app.MapGet("api/video/streaming", () =>
{
    
});
app.MapPost("/api/video/upload", async (IFormFile file, CancellationToken cancellationToken, IWebHostEnvironment webHostEnvironment) =>
{
    if (file.Length == 0) return Results.Ok();
    var directoryPath = Path.Combine(webHostEnvironment.ContentRootPath, "Conversions");
    if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
    var videoName = $"{Guid.NewGuid():N}-{file.FileName}";
    var fullPath = Path.Combine(directoryPath, videoName);
    await using (Stream fileStream = new FileStream(fullPath, FileMode.Create))
    {
        await file.CopyToAsync(fileStream, cancellationToken);
    }
    var mediaInfo = await FFmpeg.GetMediaInfo(fullPath, cancellationToken);
    var conversion = FFmpeg.Conversions.New();
    conversion.AddParameter($"-i \"{fullPath}\"");
    conversion.AddParameter("-hide_banner");
    conversion.AddParameter("-r 30");
    conversion.AddParameter("-crf 28");
    conversion.AddParameter("-c:a aac");
    conversion.AddParameter("-b:a 128k");
    conversion.AddParameter($"-hls_segment_filename \"{Path.Combine(directoryPath, $"ffmpeg-{videoName}_%03d.ts")}\"");
    conversion.AddParameter($"\"{Path.Combine(directoryPath, $"ffmpeg-{videoName}.m3u8")}\"");
    conversion.OnProgress += (_, args) =>
    {
        var percent = (int)(Math.Round(args.Duration.TotalSeconds / args.TotalLength.TotalSeconds, 2) * 100);
        app.Logger.LogInformation("{Percent}%: {@Args}", percent, args);
    };
    await conversion.Start(cancellationToken);
    return Results.Ok(new
    {
        FullPath = fullPath,
        MediaInfo = mediaInfo
    });
}).DisableAntiforgery();
app.Run();