using TranscriereYouTube.Interfaces;

namespace TranscriereYouTube.Facades
{
    public class TranscriereFacade : ITranscriereFacade
    {
        private readonly ITranscriereService _transcriereService;

        public TranscriereFacade(ITranscriereService transcriereService)
        {
            _transcriereService = transcriereService;
        }

        public string Transcrie(string audioPath, string limba)
        {
            return _transcriereService.TranscrieAudio(audioPath, limba);
        }
    }
}
