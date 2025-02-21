using TranscriereYouTube_Backend.Interfaces;

public class ErrorHandler : IErrorHandler
{
    public void HandleError(string errorMessage)
    {
        Console.WriteLine($"❌ Eroare: {errorMessage}");
    }
}