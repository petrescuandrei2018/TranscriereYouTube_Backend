using TranscriereYouTube_Backend.Interfaces;

public class WhisperService : IWhisperService
{
    private readonly IProcessRunner _processRunner;
    private readonly IErrorHandler _errorHandler;

    public WhisperService(IProcessRunner processRunner, IErrorHandler errorHandler)
    {
        _processRunner = processRunner;
        _errorHandler = errorHandler;
    }

    public async Task<Result<string>> TranscribeAudioAsync(string audioPath, string language)
    {
        var command = $"whisper \"{audioPath}\" --language {language}";
        var result = await _processRunner.RunCommandAsync("whisper", command);

        if (!result.Success)
        {
            _errorHandler.HandleError(result.ErrorMessage);
            return Result<string>.Fail($"Eroare transcriere: {result.ErrorMessage}");
        }

        return Result<string>.Ok(result.Data);
    }
}