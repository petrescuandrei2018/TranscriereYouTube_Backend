public interface IServiceFactory
{
    IDescarcatorService CreateDescarcatorService();
    IProcesorAudioService CreateProcesorAudioService();
    IProcesorVideoService CreateProcesorVideoService();
    ITranscriereService CreateTranscriereService();
}
