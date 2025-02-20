public class ProcesorAudioService : IProcesorAudioService
{
    private readonly IProcessRunner _processRunner;
    private readonly ICommandFactory _commandFactory;

    public ProcesorAudioService(IProcessRunner processRunner, ICommandFactory commandFactory)
    {
        _processRunner = processRunner;
        _commandFactory = commandFactory;
    }

    public async Task<Result<string>> ExtrageAudioAsync(string videoPath)
    {
        var audioOutput = $"{Guid.NewGuid()}.mp3";
        var command = _commandFactory.CreateFfmpegCommand(videoPath, audioOutput);

        return await _processRunner.RunCommandAsync("cmd.exe", $"/C {command}");
    }
}
