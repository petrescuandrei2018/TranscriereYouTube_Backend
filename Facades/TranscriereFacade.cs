using Microsoft.Extensions.Configuration;
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

        // ✅ 1. Descărcăm videoclipul cu progres
        Console.WriteLine("\n🔄 Descărcăm videoclipul...");
        for (int i = 0; i <= 100; i += 10)
        {
            ConsoleHelper.ShowProgressBar("Descărcare video...", i, 100);
            await Task.Delay(100); // Simulăm progresul
        }
        var descarcareResult = await DescarcaVideo(videoUrl);
        if (!descarcareResult.Success)
        {
            Console.WriteLine($"\n❌ Eroare la descărcare: {descarcareResult.ErrorMessage}");
            return Result<string>.Fail(descarcareResult.ErrorMessage);
        }
        Console.WriteLine($"\n✅ Videoclip descărcat: {descarcareResult.Data}");

        // ✅ 2. Convertim videoclipul cu progres
        Console.WriteLine("\n🔄 Convertim videoclipul...");
        for (int i = 0; i <= 100; i += 10)
        {
            ConsoleHelper.ShowProgressBar("Conversie video...", i, 100);
            await Task.Delay(100); // Simulăm progresul
        }
        var convertResult = await ConvertesteVideo(descarcareResult.Data);
        if (!convertResult.Success)
        {
            Console.WriteLine($"\n❌ Eroare la conversie: {convertResult.ErrorMessage}");
            return Result<string>.Fail(convertResult.ErrorMessage);
        }
        Console.WriteLine($"\n✅ Videoclip convertit: {convertResult.Data}");

        // ✅ 3. Extragem audio cu progres
        Console.WriteLine("\n🔄 Extragem audio...");
        for (int i = 0; i <= 100; i += 10)
        {
            ConsoleHelper.ShowProgressBar("Extragere audio...", i, 100);
            await Task.Delay(100); // Simulăm progresul
        }
        var audioResult = await ExtrageAudio(convertResult.Data);
        if (!audioResult.Success)
        {
            Console.WriteLine($"\n❌ Eroare la extragerea audio: {audioResult.ErrorMessage}");
            return Result<string>.Fail(audioResult.ErrorMessage);
        }
        Console.WriteLine($"\n✅ Audio extras: {audioResult.Data}");

        // ✅ 4. Transcriere audio cu progres
        Console.WriteLine("\n🔄 Transcriere audio...");
        for (int i = 0; i <= 100; i += 10)
        {
            ConsoleHelper.ShowProgressBar("Transcriere audio...", i, 100);
            await Task.Delay(100); // Simulăm progresul
        }
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
        var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName); // Cale completă

        Console.WriteLine($"📂 Calea completă pentru descărcare: {outputPath}");

        // ✅ Obținem calea către yt-dlp din configurație
        var ytDlpPath = _config["TranscriereSettings:YT_DLPPath"];
        if (string.IsNullOrEmpty(ytDlpPath))
        {
            Console.WriteLine("⚠️ Calea către yt-dlp nu este configurată corect în appsettings.json.");
            return Result<string>.Fail("⚠️ Calea către yt-dlp nu este configurată corect în appsettings.json.");
        }

        Console.WriteLine($"🛠️ Folosind yt-dlp de la calea: {ytDlpPath}");

        // ✅ Construim argumentele pentru yt-dlp
        var arguments = $"--no-post-overwrites -o \"{outputPath}\" \"{videoUrl}\"";

        // ✅ Afișăm comanda completă pentru verificare
        Console.WriteLine("🔧 Executăm comanda:");
        Console.WriteLine($"Executabil: {ytDlpPath}");
        Console.WriteLine($"Argumente: {arguments}");

        // ✅ Apelăm RunCommandAsync cu executabilul și argumentele separate
        var rezultat = await _processRunner.RunCommandAsync(ytDlpPath, arguments);

        // ✅ Logăm output-ul și erorile comenzii
        Console.WriteLine("📤 Output yt-dlp:");
        Console.WriteLine(rezultat.Data ?? "⚠️ Fără output de la yt-dlp.");

        if (!string.IsNullOrEmpty(rezultat.ErrorMessage))
        {
            Console.WriteLine("❌ Erori yt-dlp:");
            Console.WriteLine(rezultat.ErrorMessage);
        }

        if (!rezultat.Success)
        {
            Console.WriteLine("❌ Eroare la descărcare:");
            return Result<string>.Fail($"Eroare la descărcare: {rezultat.ErrorMessage}");
        }

        // ✅ Verificăm dacă fișierul a fost creat
        if (!File.Exists(outputPath))
        {
            // Căutăm fișierul cu extensie diferită (.webm, .mkv etc.)
            var directory = Path.GetDirectoryName(outputPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(outputPath);
            var possibleFiles = Directory.GetFiles(directory, $"{fileNameWithoutExt}.*");

            if (possibleFiles.Length > 0)
            {
                var actualFile = possibleFiles[0];
                Console.WriteLine($"✅ Fișierul descărcat găsit cu extensia corectă: {actualFile}");
                return Result<string>.Ok(actualFile);
            }

            Console.WriteLine($"⚠️ Fișierul video nu a fost găsit după descărcare la calea: {outputPath}");
            return Result<string>.Fail("⚠️ Fișierul video nu a fost găsit după descărcare.");
        }
        Console.WriteLine($"✅ Descărcare completă! Fișierul a fost salvat la: {outputPath}");
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
}
