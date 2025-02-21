public interface IFFmpegService
{
    Task<Result<string>> ConvertVideoAsync(string inputPath, string outputPath);
    Task<Result<string>> ExtractAudioAsync(string videoPath, string audioOutputPath);
}
