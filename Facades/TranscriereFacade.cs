using TranscriereYouTube.Interfaces;

namespace TranscriereYouTube.Facades
{
    /// <summary>
    /// Fațadă pentru a simplifica interacțiunea cu serviciul de transcriere.
    /// </summary>
    public class TranscriereFacade : ITranscriereFacade
    {
        private readonly ITranscriereService _transcriereService;

        public TranscriereFacade(ITranscriereService transcriereService)
        {
            _transcriereService = transcriereService;
        }

        /// <summary>
        /// Inițiază procesul de transcriere a fișierului audio în text.
        /// </summary>
        /// <param name="audioPath">Calea către fișierul audio.</param>
        /// <param name="limba">Limba în care se va face transcrierea.</param>
        /// <returns>Textul transcris.</returns>
        public string Transcrie(string audioPath, string limba)
        {
            return _transcriereService.TranscrieAudio(audioPath, limba);
        }
    }
}
