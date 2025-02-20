public interface IProcesorVideoService
{
    Task<Result<string>> ConvertVideoFormatAsync(string inputPath, string outputFormat);
    Task<Result<string>> ExtractClipAsync(string inputPath, TimeSpan startTime, TimeSpan duration);
}
