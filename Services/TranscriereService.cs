public class TranscriereService : ITranscriereService
{
    private readonly IProcessRunner _processRunner;
    private readonly ICommandFactory _commandFactory;

    public TranscriereService(IProcessRunner processRunner, ICommandFactory commandFactory)
    {
        _processRunner = processRunner;
        _commandFactory = commandFactory;
    }

    public async Task<Result<string>> TranscrieAudioAsync(string audioPath, string limba)
    {
        var command = _commandFactory.CreateWhisperCommand(audioPath, limba);
        return await _processRunner.RunCommandAsync("cmd.exe", $"/C {command}");
    }
}
