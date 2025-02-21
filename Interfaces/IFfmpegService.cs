public interface IFFmpegService
{
    Task<Result<string>> ExtractAudioAsync(string videoPath, string audioOutputPath);
    Task<Result<string>> ConvertVideoAsync(string inputPath, string outputPath);
}
