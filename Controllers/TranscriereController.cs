using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TranscriereYouTube_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Speech.V1;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

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
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"C:\\Users\\And\\Downloads\\hardy-aleph-449214-q8-f68a4c5fc542.json");
    }

    [HttpPost("simulare")]
    public async Task<IActionResult> SimulareTranscriere([FromBody] TranscriereRequest request)
    {
        if (string.IsNullOrEmpty(request.UrlOrPath))
        {
            return BadRequest("❌ URL-ul sau calea fișierului este obligatorie.");
        }

        var whisperModels = new List<string> { "tiny", "large" };
        var rezultateSimulare = new List<SimulareRezultat>();

        Console.WriteLine($"🚀 Începem simularea transcrierii pentru: {request.UrlOrPath}");

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string videoFileName = $"downloaded_video_{timestamp}.mp4";
        string audioOutputPath = Path.Combine("C:\\Temp", videoFileName);

        string audioPath = request.UrlOrPath;

        if (request.UrlOrPath.StartsWith("http://") || request.UrlOrPath.StartsWith("https://"))
        {
            Console.WriteLine("🔄 Descărcăm audio-ul de pe YouTube...");
            var downloadResult = await _videoDownloader.DownloadVideoAsync(request.UrlOrPath);

            if (!downloadResult.Success)
            {
                Console.WriteLine($"❌ Eroare la descărcare: {downloadResult.ErrorMessage}");
                return BadRequest(downloadResult.ErrorMessage);
            }

            audioPath = downloadResult.Data;
            Console.WriteLine($"✅ Audio descărcat la: {audioPath}");
        }

        string preprocessedAudioPath = Path.ChangeExtension(audioPath, ".processed.wav");
        string ffmpegPreprocessCommand = $"ffmpeg -i \"{audioPath}\" -af \"afftdn=nf=-25, dynaudnorm, highpass=f=200, lowpass=f=3000\" -ar 16000 -ac 1 \"{preprocessedAudioPath}\"";
        await _processRunner.RunCommandAsync("cmd.exe", $"/C {ffmpegPreprocessCommand}", "Preprocesare audio");
        Console.WriteLine($"✅ Audio preprocesat la: {preprocessedAudioPath}");

        string slicedAudioPath = Path.ChangeExtension(audioPath, ".sliced.wav");
        string ffmpegSliceCommand = $"ffmpeg -i \"{preprocessedAudioPath}\" -ss 00:00:15 -t 00:00:45 -c copy \"{slicedAudioPath}\"";
        await _processRunner.RunCommandAsync("cmd.exe", $"/C {ffmpegSliceCommand}", "Tăiere audio pentru detecție limbă");
        Console.WriteLine($"✅ Audio tăiat pentru detecția limbii: {slicedAudioPath}");

        string baseOutputDir = Path.Combine(Path.GetDirectoryName(audioPath), "transcrieri", Path.GetFileNameWithoutExtension(audioPath) + "_" + timestamp);
        Directory.CreateDirectory(baseOutputDir);
        Console.WriteLine($"📁 Director de ieșire creat: {baseOutputDir}");

        foreach (var model in whisperModels)
        {
            Console.WriteLine($"🎙️ Testăm modelul Whisper: {model}");

            var stopwatch = Stopwatch.StartNew();
            string modelOutputDir = Path.Combine(baseOutputDir, model);
            Directory.CreateDirectory(modelOutputDir);

            string whisperCommand = $"python -m whisper \"{slicedAudioPath}\" --task transcribe --output_dir \"{modelOutputDir}\" --model {model} --output_format txt --fp16 False --device cpu --temperature 0 --best_of 5";

            if (!string.IsNullOrEmpty(request.Language))
            {
                whisperCommand += $" --language {request.Language}";
                Console.WriteLine($"🌐 Folosim limba specificată: {request.Language}");
            }

            Console.WriteLine($"🔧 Comandă Whisper:\n{whisperCommand}");

            var processStartInfo = new ProcessStartInfo("cmd.exe", $"/C chcp 65001 && {whisperCommand}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            processStartInfo.Environment["PYTHONIOENCODING"] = "utf-8";

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.HasExited)
                {
                    await Task.Delay(5000);
                    Console.WriteLine($"⏳ Model '{model}' rulează încă... {stopwatch.Elapsed.TotalSeconds:F2}s");
                }

                await process.WaitForExitAsync();
                stopwatch.Stop();
            }

            string transcriptionFile = Directory.GetFiles(modelOutputDir, "*.txt").FirstOrDefault();
            if (string.IsNullOrEmpty(transcriptionFile))
            {
                Console.WriteLine($"⚠️ Fișierul de transcriere lipsește pentru modelul {model}");
                continue;
            }

            string transcribedText = await System.IO.File.ReadAllTextAsync(transcriptionFile);
            double wer = CalculateWER("This is the reference transcription text for accuracy comparison.", transcribedText);

            rezultateSimulare.Add(new SimulareRezultat
            {
                Model = model,
                TimpExecutie = stopwatch.Elapsed.TotalSeconds,
                Acuratete = (1 - wer) * 100,
                Transcriere = transcribedText
            });

            Console.WriteLine($"✅ Model: {model} | Timp: {stopwatch.Elapsed.TotalSeconds:F2}s | Acuratețe: {(1 - wer) * 100:F2}%");
        }

        return Ok(rezultateSimulare);
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleTranscriere([FromBody] TranscriereRequest request)
    {
        if (string.IsNullOrEmpty(request.UrlOrPath) || string.IsNullOrEmpty(request.Language))
        {
            Console.WriteLine("⚠️ Parametri lipsă: URL-ul și limba sunt obligatorii.");
            return BadRequest("❌ URL-ul și limba sunt obligatorii.");
        }

        Console.WriteLine($"🚀 Începem transcrierea cu Google pentru: {request.UrlOrPath}");

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string videoFileName = $"downloaded_video_{timestamp}.mp4";
        string audioFileName = $"downloaded_audio_{timestamp}.flac";

        string videoOutputPath = Path.Combine("C:\\Temp", videoFileName);
        string audioOutputPath = Path.Combine("C:\\Temp", audioFileName);
        string transcriptOutputPath = Path.Combine("C:\\Temp", $"transcript_{timestamp}.docx");

        try
        {
            // Descărcare video
            Console.WriteLine($"🔄 Descărcăm video din URL: {request.UrlOrPath}");
            var ytDlpCommand = $"C:\\Python313\\Scripts\\yt-dlp.exe -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{videoOutputPath}\" \"{request.UrlOrPath}\"";
            await _processRunner.RunCommandAsync("cmd.exe", $"/C {ytDlpCommand}", "Descărcare video");

            if (!System.IO.File.Exists(videoOutputPath))
            {
                Console.WriteLine($"❌ Eroare la descărcarea fișierului video.");
                return BadRequest("Nu s-a putut descărca fișierul video.");
            }

            Console.WriteLine($"✅ Fișier video salvat direct la: {videoOutputPath}");

            // Extragem audio și convertim în FLAC
            string ffmpegCommand = $"ffmpeg -i \"{videoOutputPath}\" -vn -ac 1 -acodec flac \"{audioOutputPath}\"";
            await _processRunner.RunCommandAsync("cmd.exe", $"/C {ffmpegCommand}", "Conversie audio");

            if (!System.IO.File.Exists(audioOutputPath))
            {
                Console.WriteLine($"❌ Eroare la extragerea audio-ului.");
                return BadRequest("Nu s-a putut extrage audio-ul din video.");
            }

            Console.WriteLine($"✅ Audio extras și convertit: {audioOutputPath}");

            // Configurare Google Speech-to-Text
            Console.WriteLine("⚙️ Configurăm Google Speech-to-Text...");
            var speech = SpeechClient.Create();
            var longOperation = await speech.LongRunningRecognizeAsync(new RecognitionConfig
            {
                Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
                LanguageCode = request.Language,
                SampleRateHertz = 44100,
                EnableAutomaticPunctuation = true
            }, RecognitionAudio.FromFile(audioOutputPath));

            Console.WriteLine("⏳ Procesăm transcrierea cu Google Speech-to-Text...");

            // Polling manual pentru progres
            var startTime = DateTime.Now;
            int lastProgress = -1;
            while (!longOperation.IsCompleted)
            {
                // Simulăm progresul
                var elapsed = DateTime.Now - startTime;
                int progress = Math.Min(100, (int)(elapsed.TotalSeconds * 2)); // Ajustează viteza progresului aici

                if (progress != lastProgress)
                {
                    DrawProgressBar(progress, 100);
                    lastProgress = progress;
                }

                await Task.Delay(1000); // Așteptăm un timp înainte de a verifica din nou
            }

            DrawProgressBar(100, 100);
            Console.WriteLine("\n✅ Transcriere completă!");

            // Verificăm dacă operația s-a finalizat cu succes
            if (!longOperation.IsCompleted || longOperation.Exception != null)
            {
                Console.WriteLine($"❌ Eroare în transcrierea audio cu Google Speech-to-Text.");
                if (longOperation.Exception != null)
                {
                    Console.WriteLine($"Detalii eroare: {longOperation.Exception.Message}");
                    return BadRequest($"❌ Eroare detaliată: {longOperation.Exception.Message}");
                }
                return BadRequest("❌ Eroare în transcrierea audio cu Google Speech-to-Text.");
            }

            var response = longOperation.Result;

            // Creăm documentul .docx
            Console.WriteLine("📄 Creăm documentul .docx...");
            using (WordprocessingDocument doc = WordprocessingDocument.Create(transcriptOutputPath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();

                foreach (var result in response.Results)
                {
                    foreach (var alternative in result.Alternatives)
                    {
                        var para = new Paragraph(new Run(new Text(alternative.Transcript)));
                        body.Append(para);
                        Console.WriteLine($"💬 Transcriere: {alternative.Transcript}");
                    }
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            Console.WriteLine($"✅ Transcriere salvată în: {transcriptOutputPath}");

            // Returnăm fișierul .docx
            var fileBytes = await System.IO.File.ReadAllBytesAsync(transcriptOutputPath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Path.GetFileName(transcriptOutputPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Eroare neașteptată: {ex.Message}");
            return StatusCode(500, $"❌ Eroare internă: {ex.Message}");
        }
    }

    static void DrawProgressBar(int progress, int total, int barSize = 50)
    {
        double percent = (double)progress / total;
        int filledBars = (int)(percent * barSize);
        string progressBar = $"[{new string('#', filledBars)}{new string('-', barSize - filledBars)}] {percent:P0}";
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(progressBar);
    }

    private double CalculateWER(string reference, string hypothesis)
    {
        var refWords = reference.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var hypWords = hypothesis.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int[,] distance = new int[refWords.Length + 1, hypWords.Length + 1];

        for (int i = 0; i <= refWords.Length; i++) distance[i, 0] = i;
        for (int j = 0; j <= hypWords.Length; j++) distance[0, j] = j;

        for (int i = 1; i <= refWords.Length; i++)
        {
            for (int j = 1; j <= hypWords.Length; j++)
            {
                int cost = refWords[i - 1].Equals(hypWords[j - 1], StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                distance[i, j] = new[] {
                    distance[i - 1, j] + 1,
                    distance[i, j - 1] + 1,
                    distance[i - 1, j - 1] + cost
                }.Min();
            }
        }

        double wer = (double)distance[refWords.Length, hypWords.Length] / refWords.Length;
        return Math.Max(0, Math.Min(wer, 1));
    }
}
