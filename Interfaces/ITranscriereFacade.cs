public interface ITranscriereFacade
{
    Task<Result<string>> ExecuteFullTranscription(string videoUrl, string limba);
}
