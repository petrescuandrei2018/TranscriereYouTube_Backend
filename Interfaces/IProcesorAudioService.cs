public interface IProcesorAudioService
{
    Task<Result<string>> ExtrageAudioAsync(string videoPath);
}
