using Microsoft.Extensions.Configuration;
using ShellProgressBar;
using System.Threading.Tasks;
using Utils;


public class TranscriereFacade : ITranscriereFacade
{
    private readonly IDescarcatorService _descarcatorService;
    private readonly IProcesorAudioService _procesorAudioService;
    private readonly IProcesorVideoService _procesorVideoService;
    private readonly ITranscriereService _transcriereService;

    private readonly IProcessRunner _processRunner;
    private readonly ICommandFactory _commandFactory;
    private readonly IConfiguration _config; // ✅ Adăugat pentru a accesa appsettings.json

    public TranscriereFacade(
        IDescarcatorService descarcatorService,
        IProcesorAudioService procesorAudioService,
        IProcesorVideoService procesorVideoService,
        ITranscriereService transcriereService,
        IProcessRunner processRunner,
        ICommandFactory commandFactory,
        IConfiguration config) // ✅ Injecția IConfiguration
    {
        _descarcatorService = descarcatorService;
        _procesorAudioService = procesorAudioService;
        _procesorVideoService = procesorVideoService;
        _transcriereService = transcriereService;
        _processRunner = processRunner;
        _commandFactory = commandFactory;
        _config = config; // ✅ Inițializare IConfiguration
    }

    public async Task<Result<string>> ExecuteFullTranscription(string videoUrl, string language)
    {
        Console.WriteLine("🚀 Pornim transcrierea completă...");

        // ✅ 1. Descărcăm videoclipul
        Console.WriteLine("\n🔄 Descărcăm videoclipul...");
        await SimuleazaProgres("Descărcăm videoclipul...", 100);  // ✅ Bara de progres
        var descarcareResult = await DescarcaVideo(videoUrl);
        if (!descarcareResult.Success)
        {
            Console.WriteLine($"\n❌ Eroare la descărcare: {descarcareResult.ErrorMessage}");
            return Result<string>.Fail(descarcareResult.ErrorMessage);
        }
        Console.WriteLine($"\n✅ Videoclip descărcat: {descarcareResult.Data}");

        // ✅ 2. Convertim videoclipul
        Console.WriteLine("\n🔄 Convertim videoclipul...");
        await SimuleazaProgres("Convertim videoclipul...", 100);  // ✅ Bara de progres
        var convertResult = await ConvertesteVideo(descarcareResult.Data);
        if (!convertResult.Success)
        {
            Console.WriteLine($"\n❌ Eroare la conversie: {convertResult.ErrorMessage}");
            return Result<string>.Fail(convertResult.ErrorMessage);
        }
        Console.WriteLine($"\n✅ Videoclip convertit: {convertResult.Data}");

        // ✅ 3. Extragem audio
        Console.WriteLine("\n🔄 Extragem audio...");
        await SimuleazaProgres("Extragem audio...", 100);  // ✅ Bara de progres
        var audioResult = await ExtrageAudio(convertResult.Data);
        if (!audioResult.Success)
        {
            Console.WriteLine($"\n❌ Eroare la extragerea audio: {audioResult.ErrorMessage}");
            return Result<string>.Fail(audioResult.ErrorMessage);
        }
        Console.WriteLine($"\n✅ Audio extras: {audioResult.Data}");

        // ✅ 4. Transcriere audio
        Console.WriteLine("\n🔄 Transcriem audio...");
        await SimuleazaProgres("Transcriem audio...", 100);  // ✅ Bara de progres
        var transcriereResult = await TranscrieAudio(audioResult.Data, language);
        if (!transcriereResult.Success)
        {
            Console.WriteLine($"\n❌ Eroare la transcriere: {transcriereResult.ErrorMessage}");
            return Result<string>.Fail(transcriereResult.ErrorMessage);
        }
        Console.WriteLine("\n✅ Transcriere completă!");

        return Result<string>.Ok(transcriereResult.Data);
    }

    private async Task<Result<string>> DescarcaVideo(string videoUrl)
    {
        var fileName = $"{Guid.NewGuid()}.mp4";
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var ytDlpPath = _config["TranscriereSettings:YT_DLPPath"];
        var arguments = $"-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{outputPath}\" \"{videoUrl}\"";

        var rezultat = await _processRunner.RunCommandAsync(ytDlpPath, arguments);

        if (!rezultat.Success)
        {
            return Result<string>.Fail(rezultat.ErrorMessage);
        }

        if (!File.Exists(outputPath))
        {
            return Result<string>.Fail("Fișierul nu a fost găsit după descărcare.");
        }

        return Result<string>.Ok(outputPath);
    }

    private async Task<Result<string>> ConvertesteVideo(string videoPath)
    {
        return await _procesorVideoService.ConvertVideoFormatAsync(videoPath, ".avi");
    }

    private async Task<Result<string>> ExtrageAudio(string videoPath)
    {
        return await _procesorAudioService.ExtrageAudioAsync(videoPath);
    }

    private async Task<Result<string>> TranscrieAudio(string audioPath, string language)
    {
        return await _transcriereService.TranscrieAudioAsync(audioPath, language);
    }

    public async Task SimuleazaProgres(string etapa, int maxValoare = 100)
    {
        var options = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '─',
            DisplayTimeInRealTime = true
        };

        using (var pbar = new ProgressBar(maxValoare, etapa, options))
        {
            for (int i = 0; i <= maxValoare; i++)
            {
                pbar.Tick(i);  // ✅ Actualizează progresul
                await Task.Delay(50);  // ✅ Simulează progresul
            }
        }
    }
}
