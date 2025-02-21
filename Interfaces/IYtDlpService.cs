public interface IYtDlpService
{
    Task<Result<string>> DownloadVideoAsync(string videoUrl, string outputPath);
}