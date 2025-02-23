using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/test")]
public class TestProcesExternController : ControllerBase
{
    private readonly IVideoDownloader _videoDownloader;
    private readonly IProcessRunner _processRunner;  // ✅ Adăugăm câmpul pentru IProcessRunner

    // ✅ Constructor actualizat cu IProcessRunner
    public TestProcesExternController(IVideoDownloader videoDownloader, IProcessRunner processRunner)
    {
        _videoDownloader = videoDownloader;
        _processRunner = processRunner;
    }

    [HttpGet("ytdlp")]
    public async Task<IActionResult> TestYtDlp([FromQuery] string youtubeUrl)
    {
        if (string.IsNullOrEmpty(youtubeUrl))
        {
            return BadRequest("❌ URL-ul YouTube nu este valid.");
        }

        string ytDlpPath = @"C:\Python313\Scripts\yt-dlp.exe";
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "downloaded_video.mp4");
        string arguments = $"-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{outputPath}\" \"{youtubeUrl}\"";

        // ✅ Logare detaliată a comenzii
        Console.WriteLine($"🔧 Executăm comanda yt-dlp:\n{ytDlpPath} {arguments}");

        var result = await _processRunner.RunCommandAsync(ytDlpPath, arguments, "Test descărcare video");

        if (!result.Success)
        {
            Console.WriteLine($"❌ Eroare la descărcarea videoclipului:\n{result.ErrorMessage}");
            return BadRequest($"❌ Eroare la descărcarea videoclipului: {result.ErrorMessage}");
        }

        if (!System.IO.File.Exists(outputPath))
        {
            Console.WriteLine($"⚠️ Fișierul video nu a fost creat: {outputPath}");
            return BadRequest("⚠️ Fișierul video nu a fost creat.");
        }

        Console.WriteLine($"✅ Videoclip descărcat cu succes: {outputPath}");
        return Ok($"✅ Videoclip descărcat cu succes: {outputPath}");
    }

    [HttpGet("ffmpeg")]
    public async Task<IActionResult> TestFfmpeg([FromQuery] string videoPath)
    {
        if (string.IsNullOrEmpty(videoPath))
        {
            return BadRequest("❌ Calea către fișierul video sau URL-ul este goală.");
        }

        string localVideoPath = videoPath;

        // ✅ Verificăm dacă utilizatorul a introdus un URL (ex: YouTube)
        if (videoPath.StartsWith("http://") || videoPath.StartsWith("https://"))
        {
            Console.WriteLine($"🌐 S-a detectat un URL. Începem descărcarea videoclipului de la: {videoPath}");

            // ✅ Descărcăm videoclipul folosind yt-dlp
            var downloadResult = await _videoDownloader.DownloadVideoAsync(videoPath);

            if (!downloadResult.Success)
            {
                return BadRequest($"❌ Eroare la descărcarea videoclipului: {downloadResult.ErrorMessage}");
            }

            localVideoPath = downloadResult.Data;
            Console.WriteLine($"✅ Videoclip descărcat la: {localVideoPath}");
        }

        // ✅ Verificăm dacă fișierul există local
        if (!System.IO.File.Exists(localVideoPath))
        {
            return BadRequest("❌ Calea către fișierul video nu este validă sau nu există.");
        }

        // ✅ Începem extragerea audio folosind ffmpeg cu filtre pentru reducerea zgomotului și normalizare
        var audioOutputPath = Path.ChangeExtension(localVideoPath, ".mp3");
        string ffmpegPath = @"C:\FFmpeg\bin\ffmpeg.exe";

        // ✅ Adăugăm filtrele afftdn și dynaudnorm pentru îmbunătățirea calității audio
        string arguments = $"-y -i \"{localVideoPath}\" -af \"afftdn, dynaudnorm\" -vn -q:a 0 -map a \"{audioOutputPath}\"";

        // ✅ Logare detaliată a comenzii
        Console.WriteLine($"🔧 Executăm comanda ffmpeg:\n{ffmpegPath} {arguments}");

        var result = await _processRunner.RunCommandAsync(ffmpegPath, arguments, "Extragere audio cu îmbunătățiri");

        if (!result.Success)
        {
            Console.WriteLine($"❌ Eroare la extragerea audio:\n{result.ErrorMessage}");
            return BadRequest($"❌ Eroare la extragerea audio: {result.ErrorMessage}");
        }

        if (!System.IO.File.Exists(audioOutputPath))
        {
            Console.WriteLine($"⚠️ Fișierul audio nu a fost creat: {audioOutputPath}");
            return BadRequest("⚠️ Fișierul audio nu a fost creat.");
        }

        Console.WriteLine($"✅ Audio extras și procesat cu succes: {audioOutputPath}");
        return Ok($"✅ Audio extras și procesat cu succes: {audioOutputPath}");
    }

}
