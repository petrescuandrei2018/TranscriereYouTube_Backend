// YtDlpService.cs
using TranscriereYouTube_Backend.Interfaces;

public class YtDlpService : IYtDlpService
{
    private readonly IProcessRunner _processRunner;
    private readonly IErrorHandler _errorHandler;

    public YtDlpService(IProcessRunner processRunner, IErrorHandler errorHandler)
    {
        _processRunner = processRunner;
        _errorHandler = errorHandler;
    }

    public async Task<Result<string>> DownloadVideoAsync(string videoUrl, string outputPath)
    {
        var command = $"yt-dlp -o \"{outputPath}\" \"{videoUrl}\"";
        var result = await _processRunner.RunCommandAsync("yt-dlp", command);

        if (!result.Success)
        {
            _errorHandler.HandleError(result.ErrorMessage);
            return Result<string>.Fail($"Eroare descărcare video: {result.ErrorMessage}");
        }

        return Result<string>.Ok(outputPath);
    }
}