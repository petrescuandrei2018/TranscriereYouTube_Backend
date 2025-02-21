using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class VideoDownloader : IVideoDownloader
{
    private readonly IProcessRunner _processRunner;

    public VideoDownloader(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    // ✅ Metodă pentru a extrage VideoID din URL
    private string ExtractVideoId(string url)
    {
        var regex = new Regex(@"(?:v=|\/)([0-9A-Za-z_-]{11}).*");
        var match = regex.Match(url);
        return match.Success ? match.Groups[1].Value : null;
    }

    // ✅ Metodă pentru a curăța numele fișierului
    private string GetSafeFilename(string filename)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            filename = filename.Replace(c, '_');
        }
        return filename;
    }

    // ✅ Descărcăm videoclipul de pe YouTube
    public async Task<Result<string>> DownloadVideoAsync(string youtubeUrl)
    {
        Console.WriteLine($"🚀 Descărcare video din URL: {youtubeUrl}");

        string videoId = ExtractVideoId(youtubeUrl);
        if (string.IsNullOrEmpty(videoId))
        {
            Console.WriteLine("❌ URL invalid. Nu s-a putut extrage Video ID.");
            return Result<string>.Fail("❌ URL invalid. Nu s-a putut extrage Video ID.");
        }

        string fileName = GetSafeFilename($"{videoId}.mp4");
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        string ytDlpPath = @"C:\Python313\Scripts\yt-dlp.exe";
        string arguments = $"-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{outputPath}\" \"{youtubeUrl}\"";

        Console.WriteLine($"📁 Executăm comanda yt-dlp:\n{ytDlpPath} {arguments}");

        var rezultat = await _processRunner.RunCommandAsync(ytDlpPath, arguments);

        if (!rezultat.Success)
        {
            Console.WriteLine($"❌ Eroare yt-dlp: {rezultat.ErrorMessage}");
            return Result<string>.Fail($"❌ Eroare la descărcare: {rezultat.ErrorMessage}");
        }

        if (!File.Exists(outputPath))
        {
            Console.WriteLine("❌ Fișierul video nu a fost creat.");
            return Result<string>.Fail("❌ Fișierul video nu a fost creat.");
        }

        Console.WriteLine($"✅ Descărcare completă: {outputPath}");
        return Result<string>.Ok(outputPath);
    }

    // ✅ Extragem doar audio-ul din videoclip
    public async Task<Result<string>> ExtractAudioAsync(string videoPath)
    {
        if (!File.Exists(videoPath))
        {
            Console.WriteLine($"❌ Fișierul video nu există: {videoPath}");
            return Result<string>.Fail("❌ Fișierul video nu există.");
        }

        // ✅ Conversie Video înainte de extragere audio
        var conversionResult = await _processRunner.ConvertAv1ToH264Async(videoPath);
        if (!conversionResult.Success)
            return Result<string>.Fail(conversionResult.ErrorMessage);

        string convertedVideoPath = conversionResult.Data;

        // ✅ Extragere Audio
        string ffmpegPath = @"C:\FFmpeg\bin\ffmpeg.exe";
        string audioOutput = Path.ChangeExtension(convertedVideoPath, ".mp3");
        string arguments = $"-y -i \"{convertedVideoPath}\" -vn -q:a 0 -map a \"{audioOutput}\"";

        Console.WriteLine($"🎵 Începem extragerea audio...");
        var audioResult = await _processRunner.RunCommandAsync(ffmpegPath, arguments, "Extragere audio din video");

        if (!audioResult.Success)
        {
            Console.WriteLine($"❌ Eroare la extragerea audio: {audioResult.ErrorMessage}");
            return Result<string>.Fail(audioResult.ErrorMessage);
        }

        Console.WriteLine($"✅ Audio extras cu succes: {audioOutput}");
        return Result<string>.Ok(audioOutput);
    }
}
