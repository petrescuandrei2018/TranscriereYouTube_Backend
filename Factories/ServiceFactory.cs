using Microsoft.Extensions.Configuration;
using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Services;

namespace TranscriereYouTube.Factories
{
    public class ServiceFactory
    {
        private readonly IConfiguration _configuration;

        public ServiceFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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
        public ITranscriereService CreateTranscriereService() => new TranscriereService(_configuration);
    }
}
