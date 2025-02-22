using TranscriereYouTube_Backend.Interfaces;

public class ErrorHandler : IErrorHandler
{
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(ILogger<ErrorHandler> logger)
    {
        _logger = logger;
    }

    public void HandleError(string errorMessage)
    {
        // ✅ Loghează eroarea în sistemul de loguri
        _logger.LogError($"🚨 Eroare: {errorMessage}");

        // Poți adăuga și alte mecanisme de notificare aici (ex: email, servicii externe, etc.)
    }
}
