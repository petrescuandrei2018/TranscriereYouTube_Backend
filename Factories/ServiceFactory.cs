using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Services;

namespace TranscriereYouTube.Factories
{
    public class ServiceFactory
    {
        /// <summary>
        /// Creează o instanță a serviciului responsabil cu descărcarea videoclipurilor.
        /// </summary>
        public IDescarcatorService CreateDescarcatorService() => new DescarcatorService();

        /// <summary>
        /// Creează o instanță a serviciului responsabil cu procesarea fișierelor video și audio.
        /// </summary>
        public IProcesorVideoService CreateProcesorVideoService() => new ProcesorVideoService();

        /// <summary>
        /// Creează o instanță a serviciului responsabil cu transcrierea audio în text.
        /// </summary>
        public ITranscriereService CreateTranscriereService() => new TranscriereService();
    }
}
