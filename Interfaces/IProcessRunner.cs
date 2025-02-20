public interface IProcessRunner
{
    Task<Result<string>> RunCommandAsync(string executable, string arguments);
}
