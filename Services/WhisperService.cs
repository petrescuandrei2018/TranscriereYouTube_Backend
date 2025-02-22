using System.Diagnostics;
using System.Threading.Tasks;

public class WhisperService : IWhisperService
{
    private readonly ICommandFactory _commandFactory;
    private readonly IProcessRunner _processRunner;

    public WhisperService(ICommandFactory commandFactory, IProcessRunner processRunner)
    {
        _commandFactory = commandFactory;
        _processRunner = processRunner;
    }

    public async Task<Result<string>> TranscribeAudioAsync(string audioPath, string language)
    {
        // ✅ Generăm comanda folosind CommandFactory fără enum-uri
        string whisperCommand = _commandFactory.CreateWhisperCommand(audioPath, language);

        Console.WriteLine($"🎙️ Executăm comanda Whisper: {whisperCommand}");

        // ✅ Rulăm comanda cu ProcessRunner
        var result = await _processRunner.RunCommandAsync("cmd.exe", $"/C {whisperCommand}", "Transcriere audio");

        if (!result.Success)
        {
            Console.WriteLine($"❌ Eroare la transcriere: {result.ErrorMessage}");
            return Result<string>.Fail(result.ErrorMessage);
        }

        Console.WriteLine($"✅ Transcriere completă: {result.Data}");
        return Result<string>.Ok(result.Data);
    }
}
