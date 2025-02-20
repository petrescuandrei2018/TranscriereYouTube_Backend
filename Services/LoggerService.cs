using TranscriereYouTube_Backend.Interfaces;

public class LoggerService : ILoggerService
{
    public void LogInfo(string message)
    {
        Console.WriteLine($"[INFO] {message}");
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"[WARNING] {message}");
    }

    public void LogError(string message)
    {
        Console.WriteLine($"[ERROR] {message}");
    }

    public void LogCritical(string message)
    {
        Console.WriteLine($"[CRITICAL] {message}");
    }
}
