public interface IVideoDownloader
{
    Task<Result<string>> DownloadVideoAsync(string youtubeUrl);
    Task<Result<string>> ExtractAudioAsync(string videoPath, string audioOutputPath);
}
