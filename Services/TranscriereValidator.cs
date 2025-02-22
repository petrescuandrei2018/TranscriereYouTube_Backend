using TranscriereYouTube_Backend.Models;

public class TranscriereValidator : ITranscriereValidator
{
    private readonly HashSet<string> _limbiSuportate = new() { "ro", "en", "fr" };

    public Result<bool> ValideazaRequest(TranscriereRequest request)
    {
        if (string.IsNullOrEmpty(request.UrlOrPath))
            return Result<bool>.Fail("⚠️ URL-ul videoclipului este necesar.");

        if (!_limbiSuportate.Contains(request.Language.ToString().ToLower()))
            return Result<bool>.Fail($"❌ language '{request.Language}' nu este suportată. Limbile disponibile sunt: ro, en.");

        return Result<bool>.Ok(true);
    }
}
