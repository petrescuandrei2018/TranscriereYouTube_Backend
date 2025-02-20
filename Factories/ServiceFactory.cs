public class ServiceFactory : IServiceFactory
{
    private readonly IProcessRunner _processRunner;
    private readonly ICommandFactory _commandFactory;

    public ServiceFactory(IProcessRunner processRunner, ICommandFactory commandFactory)
    {
        _processRunner = processRunner;
        _commandFactory = commandFactory;
    }

    public IDescarcatorService CreateDescarcatorService()
    {
        return new DescarcatorService(_processRunner, _commandFactory);
    }

    public IProcesorAudioService CreateProcesorAudioService()
    {
        return new ProcesorAudioService(_processRunner, _commandFactory);
    }

    public IProcesorVideoService CreateProcesorVideoService()
    {
        return new ProcesorVideoService(_processRunner, _commandFactory);
    }

    public ITranscriereService CreateTranscriereService()
    {
        return new TranscriereService(_processRunner, _commandFactory);
    }
}
