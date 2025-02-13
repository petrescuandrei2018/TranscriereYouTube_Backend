using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Services;

namespace TranscriereYouTube.Factories
{
    public class ServiceFactory
    {
        public IDescarcatorService CreateDescarcatorService() => new DescarcatorService();
        public IProcesorVideoService CreateProcesorVideoService() => new ProcesorVideoService();
        public ITranscriereService CreateTranscriereService() => new TranscriereService();
    }
}