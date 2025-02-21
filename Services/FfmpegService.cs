public class FFmpegService : IFFmpegService
{
    private readonly IProcessRunner _processRunner;

    public FFmpegService(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<Result<string>> ConvertVideoAsync(string inputPath, string outputPath)
    {
        string arguments = $"-i \"{inputPath}\" -c:v libx264 -preset fast -crf 23 -c:a copy \"{outputPath}\"";
        var result = await _processRunner.RunCommandAsync("ffmpeg", arguments, "Conversie video");

        return result.Success ? Result<string>.Ok(outputPath) : Result<string>.Fail(result.ErrorMessage);
    }

    public async Task<Result<string>> ExtractAudioAsync(string videoPath, string audioOutputPath)
    {
        string arguments = $"-i \"{videoPath}\" -q:a 0 -map a \"{audioOutputPath}\"";
        var result = await _processRunner.RunCommandAsync("ffmpeg", arguments, "Extragere audio");

        return result.Success ? Result<string>.Ok(audioOutputPath) : Result<string>.Fail(result.ErrorMessage);
    }
}
