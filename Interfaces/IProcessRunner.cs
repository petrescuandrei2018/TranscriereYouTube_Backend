public interface IProcessRunner
{
    Task<Result<string>> RunCommandAsync(string executable, string arguments, string taskDescription = "Proces în desfășurare");
    Task<Result<string>> ConvertAv1ToH264Async(string inputPath);
}
