public interface ILoggerService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogCritical(string message);
}
