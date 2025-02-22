public interface ITranscriereService
{
    Task<Result<string>> TranscrieAudioAsync(string audioPath, string language);
}
