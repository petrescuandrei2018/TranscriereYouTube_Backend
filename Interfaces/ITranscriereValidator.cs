using TranscriereYouTube_Backend.Models;

public interface ITranscriereValidator
{
    Result<bool> ValideazaRequest(TranscriereRequest request);
}
