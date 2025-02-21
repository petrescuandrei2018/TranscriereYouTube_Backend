using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/transcriere")]
public class TranscriereController : ControllerBase
{
    private readonly IYtDlpService _ytDlpService;
    private readonly IFFmpegService _ffmpegService;
    private readonly IWhisperService _whisperService;
    private readonly ILogger<TranscriereController> _logger;

    public TranscriereController(IYtDlpService ytDlpService, IFFmpegService ffmpegService, IWhisperService whisperService, ILogger<TranscriereController> logger)
    {
        _ytDlpService = ytDlpService;
        _ffmpegService = ffmpegService;
        _whisperService = whisperService;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartTranscription([FromBody] TranscriereRequest request)
    {
        _logger.LogInformation("🚀 Începem transcrierea pentru URL-ul: {VideoUrl}", request.VideoUrl);

        // Descărcare Video
        var downloadResult = await _ytDlpService.DownloadVideoAsync(request.VideoUrl, "video.mp4");
        if (!downloadResult.Success)
        {
            _logger.LogError("❌ Eroare la descărcarea videoclipului: {Error}", downloadResult.ErrorMessage);
            return BadRequest(downloadResult.ErrorMessage);
        }
        _logger.LogInformation("✅ Videoclip descărcat cu succes.");

        // Conversie Video
        var convertResult = await _ffmpegService.ConvertVideoAsync("video.mp4", "converted.mp4");
        if (!convertResult.Success)
        {
            _logger.LogError("❌ Eroare la conversia videoclipului: {Error}", convertResult.ErrorMessage);
            return BadRequest(convertResult.ErrorMessage);
        }
        _logger.LogInformation("✅ Conversie video completă.");

        // Transcriere Audio
        var transcribeResult = await _whisperService.TranscribeAudioAsync("converted.mp4", request.Language);
        if (!transcribeResult.Success)
        {
            _logger.LogError("❌ Eroare la transcrierea audio: {Error}", transcribeResult.ErrorMessage);
            return BadRequest(transcribeResult.ErrorMessage);
        }
        _logger.LogInformation("✅ Transcriere completă.");

        return Ok(new { Transcription = transcribeResult.Data });
    }
}
