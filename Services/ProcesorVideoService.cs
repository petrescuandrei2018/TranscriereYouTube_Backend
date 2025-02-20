public class ProcesorVideoService : IProcesorVideoService
{
    private readonly IProcessRunner _processRunner;
    private readonly ICommandFactory _commandFactory;

    public ProcesorVideoService(IProcessRunner processRunner, ICommandFactory commandFactory)
    {
        _processRunner = processRunner;
        _commandFactory = commandFactory;
    }

    public async Task<Result<string>> ConvertVideoFormatAsync(string inputPath, string outputFormat)
    {
        var outputPath = Path.ChangeExtension(inputPath, outputFormat);
        var command = $"ffmpeg -i \"{inputPath}\" \"{outputPath}\"";

        var result = await _processRunner.RunCommandAsync("cmd.exe", $"/C {command}");

        return result.Success ? Result<string>.Ok(outputPath) : Result<string>.Fail(result.ErrorMessage);
    }

    public async Task<Result<string>> ExtractClipAsync(string inputPath, TimeSpan startTime, TimeSpan duration)
    {
        var outputPath = $"{Path.GetFileNameWithoutExtension(inputPath)}_clip.mp4";
        var command = $"ffmpeg -i \"{inputPath}\" -ss {startTime} -t {duration} -c copy \"{outputPath}\"";

        var result = await _processRunner.RunCommandAsync("cmd.exe", $"/C {command}");

        return result.Success ? Result<string>.Ok(outputPath) : Result<string>.Fail(result.ErrorMessage);
    }
}
