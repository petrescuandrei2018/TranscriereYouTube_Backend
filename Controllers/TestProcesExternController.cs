using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/test")]
public class TestProcesExternController : ControllerBase
{
    private readonly IVideoDownloader _videoDownloader;

    public TestProcesExternController(IVideoDownloader videoDownloader)
    {
        _videoDownloader = videoDownloader;
    }

    [HttpGet("yt-dlp")]
    public async Task<IActionResult> TestYtDlp([FromQuery] string videoUrl)
    {
        var result = await _videoDownloader.DownloadVideoAsync(videoUrl);
        return result.Success ? Ok($"✅ Descărcare yt-dlp completă: {result.Data}") : BadRequest($"❌ Eroare: {result.ErrorMessage}");
    }

    [HttpGet("ffmpeg")]
    public async Task<IActionResult> TestFfmpeg([FromQuery] string videoPath)
    {
        var result = await _videoDownloader.ExtractAudioAsync(videoPath);
        return result.Success ? Ok($"✅ Conversie ffmpeg completă: {result.Data}") : BadRequest($"❌ Eroare: {result.ErrorMessage}");
    }
}