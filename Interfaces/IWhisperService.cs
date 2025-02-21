public interface IWhisperService
{
    Task<Result<string>> TranscribeAudioAsync(string audioPath, string language);
}