using TranscriereYouTube_Backend.Interfaces;

public class ErrorHandler : IErrorHandler
{
    public void HandleError(string errorMessage)
    {
        Console.WriteLine($"[ERROR HANDLER]: {errorMessage}");
        // Aici poți adăuga mai multă logică (ex: logare în fișier, raportare erori, etc.)
    }
}
