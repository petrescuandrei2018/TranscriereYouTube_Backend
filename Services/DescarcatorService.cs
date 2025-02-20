public class DescarcatorService : IDescarcatorService
{
    private readonly IProcessRunner _processRunner;
    private readonly ICommandFactory _commandFactory;
    private readonly IConfiguration _config; // ✅ Adăugăm variabila de configurare

    public DescarcatorService(IProcessRunner processRunner, ICommandFactory commandFactory, IConfiguration config)
    {
        _processRunner = processRunner;
        _commandFactory = commandFactory;
        _config = config; // ✅ Injectăm configurația
    }

    public DescarcatorService(IProcessRunner processRunner, ICommandFactory commandFactory)
    {
        _processRunner = processRunner;
        _commandFactory = commandFactory;
    }

    public async Task<Result<string>> DescarcaVideoAsync(string videoUrl)
    {
        var outputPath = $"{Guid.NewGuid()}.mp4";
        var ytDlpPath = _config["TranscriereSettings:YT_DLPPath"];
        var arguments = $"--no-post-overwrites -o \"{outputPath}\" \"{videoUrl}\"";

        Console.WriteLine($"Executabil: {ytDlpPath}");
        Console.WriteLine($"Argumente: {arguments}");

        var rezultat = await _processRunner.RunCommandAsync(ytDlpPath, arguments);

        return rezultat.Success
            ? Result<string>.Ok(outputPath)
            : Result<string>.Fail(rezultat.ErrorMessage);
    }
}
