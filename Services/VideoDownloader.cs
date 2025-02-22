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

        string outputPath = Path.Combine("C:\\Temp", "downloaded_video.mp4");
        string ytDlpPath = @"C:\Python313\Scripts\yt-dlp.exe";
        string arguments = $"-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{outputPath}\" \"{youtubeUrl}\"";

        // ✅ Logare comandă yt-dlp
        Console.WriteLine($"🔧 Comandă yt-dlp: {ytDlpPath} {arguments}");

        var rezultat = await _processRunner.RunCommandAsync(ytDlpPath, arguments, "Descărcare video");

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
    public async Task<Result<string>> ExtractAudioAsync(string videoPath, string audioOutputPath)
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

        // ✅ Extragere Audio folosind ffmpeg
        string ffmpegPath = @"C:\FFmpeg\bin\ffmpeg.exe";
        string arguments = $"-y -i \"{convertedVideoPath}\" -vn -q:a 0 -map a \"{audioOutputPath}\"";

        Console.WriteLine($"🎵 Începem extragerea audio...");
        var audioResult = await _processRunner.RunCommandAsync(ffmpegPath, arguments, "Extragere audio din video");

        // ✅ Validare după rularea comenzii
        if (!audioResult.Success || !File.Exists(audioOutputPath))
        {
            var errorMessage = audioResult.Success ? "⚠️ Fișierul audio nu a fost creat." : audioResult.ErrorMessage;
            Console.WriteLine($"❌ Eroare la extragerea audio: {errorMessage}");
            return Result<string>.Fail(errorMessage);
        }

        Console.WriteLine($"✅ Audio extras cu succes: {audioOutputPath}");
        return Result<string>.Ok(audioOutputPath);
    }
}
