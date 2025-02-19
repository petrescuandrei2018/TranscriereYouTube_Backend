﻿public class TranscriereValidator : ITranscriereValidator
{
    private readonly HashSet<string> _limbiSuportate = new() { "ro", "en" };

    public Result<bool> ValideazaRequest(TranscriereRequest request)
    {
        if (string.IsNullOrEmpty(request.VideoPath))
            return Result<bool>.Fail("⚠️ URL-ul videoclipului este necesar.");

        if (!_limbiSuportate.Contains(request.Language.ToLower()))
            return Result<bool>.Fail($"❌ Limba '{request.Language}' nu este suportată. Limbile disponibile sunt: ro, en.");

        return Result<bool>.Ok(true);
    }
}
