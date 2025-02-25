using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TranscriereYouTube_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Speech.V1;
using CliWrap;
using Serilog;
using CliWrap.Buffered;

[ApiController]
[Route("api/transcriere")]
public class TranscriereController : ControllerBase
{
    private readonly IVideoDownloader _videoDownloader;
    private readonly IProcessRunner _processRunner;

    public TranscriereController(IVideoDownloader videoDownloader, IProcessRunner processRunner)
    {
        _videoDownloader = videoDownloader;
        _processRunner = processRunner;

        // Configurare Serilog pentru logging structurat
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\\Users\\And\\Downloads\\hardy-aleph-449214-q8-f68a4c5fc542.json");
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleTranscriere([FromBody] TranscriereRequest request)
    {
        if (string.IsNullOrEmpty(request.UrlOrPath) || string.IsNullOrEmpty(request.Language))
        {
            Log.Warning("⚠️ Parametri lipsă: URL-ul și limba sunt obligatorii.");
            return BadRequest("❌ URL-ul și limba sunt obligatorii.");
        }

        Log.Information("🚀 Începem transcrierea cu Google pentru: {Url}", request.UrlOrPath);

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string videoFileName = $"downloaded_video_{timestamp}.mp4";
        string audioFileName = $"downloaded_audio_{timestamp}.wav";

        string videoOutputPath = Path.Combine("C:\\Temp", videoFileName);
        string audioOutputPath = Path.Combine("C:\\Temp", audioFileName);
        string transcriptOutputPath = Path.Combine("C:\\Temp", $"transcript_{timestamp}.txt");

        try
        {
            // Descărcăm video folosind yt-dlp (asincron cu CliWrap)
            await DownloadVideoAsync(request.UrlOrPath, videoOutputPath);

            if (!System.IO.File.Exists(videoOutputPath))
            {
                Log.Error("❌ Eroare la descărcarea fișierului video.");
                return BadRequest("Nu s-a putut descărca fișierul video.");
            }

            // Extragem audio și normalizăm volumul
            await ConvertAndNormalizeAudioAsync(videoOutputPath, audioOutputPath);

            if (!System.IO.File.Exists(audioOutputPath))
            {
                Log.Error("❌ Eroare la extragerea audio-ului.");
                return BadRequest("Nu s-a putut extrage audio-ul din video.");
            }

            Log.Information("✅ Audio extras și convertit: {AudioPath}", audioOutputPath);

            // Verificăm durata și dimensiunea audio-ului pentru fragmentare inteligentă
            var audioInfo = await GetAudioInfoAsync(audioOutputPath);
            List<string> fragments;

            if (audioInfo.Duration.TotalMinutes > 1 || audioInfo.FileSize > 10 * 1024 * 1024)
            {
                fragments = await FragmentAudioFileAsync(audioOutputPath, 30); // Fragmente de 30 secunde
            }
            else
            {
                fragments = new List<string> { audioOutputPath };
            }

            // Transcriem în batch-uri
            var fullTranscription = await TranscribeInBatchesAsync(fragments, request.Language, batchSize: 5);

            // Salvăm transcrierea într-un fișier .txt
            await System.IO.File.WriteAllTextAsync(transcriptOutputPath, fullTranscription);
            Log.Information("✅ Transcriere salvată în: {TranscriptPath}", transcriptOutputPath);

            // Returnăm fișierul .txt pentru descărcare
            var fileBytes = await System.IO.File.ReadAllBytesAsync(transcriptOutputPath);
            return File(fileBytes, "text/plain", Path.GetFileName(transcriptOutputPath));
        }
        catch (Exception ex)
        {
            Log.Fatal("❌ Eroare neașteptată: {ErrorMessage}", ex.Message);
            return StatusCode(500, $"❌ Eroare internă: {ex.Message}");
        }
    }

    private async Task DownloadVideoAsync(string url, string outputPath)
    {
        var ytDlpCommand = $@"C:\Python313\Scripts\yt-dlp.exe -f ""bestaudio[ext=m4a]+bestvideo[ext=mp4]/mp4"" -o ""{outputPath}"" {url}";

        Log.Information("⬇️ Descarcăm video cu yt-dlp...");

        try
        {
            var cmd = Cli.Wrap("cmd")
                .WithArguments($"/C {ytDlpCommand}")
                .WithValidation(CommandResultValidation.None);

            await cmd.WithStandardOutputPipe(PipeTarget.ToDelegate(line =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Log.Information($"yt-dlp Output: {line}");
                }
            }))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(line =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    Log.Error($"yt-dlp Error: {line}");
                }
            }))
            .ExecuteAsync();

            if (!System.IO.File.Exists(outputPath))
            {
                throw new Exception("❌ Descărcarea a eșuat. Fișierul video nu a fost găsit.");
            }


            Log.Information("✅ Video descărcat cu succes la: {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            Log.Error("❌ Eroare în timpul descărcării: {Message}", ex.Message);
            throw;
        }
    }

    private async Task ConvertAndNormalizeAudioAsync(string videoPath, string audioPath)
    {
        var ffmpegCommand = $"ffmpeg -i \"{videoPath}\" -vn -ac 1 -ar 16000 -acodec pcm_s16le -af loudnorm \"{audioPath}\"";

        Log.Information("🎵 Convertim și normalizăm audio cu ffmpeg...");
        await Cli.Wrap("cmd")
            .WithArguments($"/C {ffmpegCommand}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();
    }

    private async Task<AudioInfo> GetAudioInfoAsync(string audioPath)
    {
        var ffprobeCommand = $"ffprobe -v error -show_entries format=duration,size -of default=noprint_wrappers=1:nokey=1 \"{audioPath}\"";
        var result = await Cli.Wrap("cmd")
            .WithArguments($"/C {ffprobeCommand}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        var lines = result.StandardOutput.Split('\n');
        double duration = double.Parse(lines[0]);
        long fileSize = long.Parse(lines[1]);

        return new AudioInfo(TimeSpan.FromSeconds(duration), fileSize);
    }

    private async Task<List<string>> FragmentAudioFileAsync(string inputFile, int segmentDuration)
    {
        var fragments = new List<string>();
        var tempDir = Path.Combine(Path.GetDirectoryName(inputFile), "fragments");
        Directory.CreateDirectory(tempDir);

        var outputPattern = Path.Combine(tempDir, "fragment_%03d.wav");
        var ffmpegCommand = $"ffmpeg -i \"{inputFile}\" -f segment -segment_time {segmentDuration} -c copy \"{outputPattern}\"";

        Log.Information("🔪 Fragmentăm audio-ul cu ffmpeg...");
        await Cli.Wrap("cmd")
            .WithArguments($"/C {ffmpegCommand}")
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        fragments.AddRange(Directory.GetFiles(tempDir, "fragment_*.wav"));
        Log.Information("✅ Fragmentare completă. Total fragmente: {Count}", fragments.Count);

        return fragments;
    }

    private async Task<string> TranscribeInBatchesAsync(List<string> fragments, string language, int batchSize)
    {
        var speechClient = SpeechClient.Create();
        var transcriptions = new List<string>();

        for (int i = 0; i < fragments.Count; i += batchSize)
        {
            var batch = fragments.Skip(i).Take(batchSize).ToList();
            Log.Information("📝 Transcriem batch-ul {BatchNumber}/{TotalBatches}", (i / batchSize) + 1, (int)Math.Ceiling((double)fragments.Count / batchSize));

            var tasks = batch.Select(async fragment =>
            {
                try
                {
                    var recognitionConfig = new RecognitionConfig
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = language,
                        EnableAutomaticPunctuation = true,
                        AudioChannelCount = 1,
                        UseEnhanced = true
                    };

                    var longOperation = await speechClient.LongRunningRecognizeAsync(
                        recognitionConfig,
                        RecognitionAudio.FromFile(fragment)
                    );

                    var completedOperation = await longOperation.PollUntilCompletedAsync();

                    if (completedOperation == null || completedOperation.IsFaulted)
                    {
                        Log.Warning("⚠️ Eroare la transcrierea fragmentului {Fragment}", fragment);
                        return string.Empty;
                    }

                    var response = completedOperation.Result;
                    var transcript = string.Join(" ", response.Results.Select(r => r.Alternatives.First().Transcript));

                    Log.Information("✅ Transcriere completă pentru fragmentul: {Fragment}", fragment);
                    return transcript;
                }
                catch (Exception ex)
                {
                    Log.Error("❌ Eroare la transcrierea fragmentului {Fragment}: {ErrorMessage}", fragment, ex.Message);
                    return string.Empty;
                }
            });

            var results = await Task.WhenAll(tasks);
            transcriptions.AddRange(results.Where(t => !string.IsNullOrEmpty(t)));
        }

        return string.Join(" ", transcriptions);
    }

    private record AudioInfo(TimeSpan Duration, long FileSize);
}
