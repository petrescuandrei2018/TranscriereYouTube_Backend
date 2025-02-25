﻿public class CommandFactory : ICommandFactory
{
    private readonly IConfiguration _config;

    public CommandFactory(IConfiguration config)
    {
        _config = config;
    }

    // ✅ Comandă corectă pentru yt-dlp
    public string CreateYtDlpCommand(string videoUrl, string outputPath)
    {
        var ytDlpPath = _config["TranscriereSettings:YT_DLPPath"];
        if (string.IsNullOrEmpty(ytDlpPath))
            throw new Exception("⚠️ Calea către yt-dlp nu este configurată corect în appsettings.json.");

        // ✅ Curățăm ghilimelele în exces din calea executabilului
        ytDlpPath = ytDlpPath.Trim('"');

        // ✅ Construim comanda cu ghilimele corecte
        var command = $"\"{ytDlpPath}\" --no-post-overwrites -o \"{outputPath}\" \"{videoUrl}\"";

        // ✅ Afișăm comanda pentru verificare
        Console.WriteLine($"⚡ Comanda yt-dlp generată: {command}");

        return command;
    }

    // ✅ Comandă corectă pentru ffmpeg cu filtre audio pentru reducerea zgomotului și normalizare
    public string CreateFfmpegCommand(string videoPath, string audioOutputPath)
    {
        var ffmpegPath = _config["TranscriereSettings:FFmpegPath"];
        if (string.IsNullOrEmpty(ffmpegPath))
            throw new Exception("⚠️ Calea către ffmpeg nu este configurată corect în appsettings.json.");

        var videoFullPath = Path.GetFullPath(videoPath).Replace("\"", "\\\"");
        var audioFullPath = Path.GetFullPath(audioOutputPath).Replace("\"", "\\\"");

        Console.WriteLine($"📂 Cale video: \"{videoFullPath}\"");
        Console.WriteLine($"🎵 Cale audio: \"{audioFullPath}\"");

        // ✅ Adăugăm filtrele afftdn și dynaudnorm
        string arguments = $"-i \"{videoFullPath}\" -af \"afftdn, dynaudnorm\" -q:a 0 -map a \"{audioFullPath}\"";

        Console.WriteLine($"⚡ Executăm comanda: \"{ffmpegPath}\" {arguments}");

        return $"\"{ffmpegPath}\" {arguments}";
    }

    // ✅ Comandă corectă pentru Whisper
    public string CreateWhisperCommand(string audioPath, string language = null)
    {
        var whisperPath = _config["TranscriereSettings:WhisperPath"];
        if (string.IsNullOrEmpty(whisperPath))
            throw new Exception("⚠️ Calea către Whisper nu este configurată corect în appsettings.json.");

        // Construim comanda fără parametrul --language pentru a permite detecția automată
        string command = $"\"{whisperPath}\" \"{audioPath}\" --task transcribe";

        // Dacă limba este specificată manual, o adăugăm în comandă
        if (!string.IsNullOrEmpty(language))
        {
            command += $" --language {language}";
        }

        return command;
    }
}
