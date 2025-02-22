using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TranscriereYouTube_Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }

    [HttpPost("simulare")]
    public async Task<IActionResult> SimulareTranscriere([FromBody] TranscriereRequest request)
    {
        if (string.IsNullOrEmpty(request.UrlOrPath))
        {
            return BadRequest("❌ URL-ul sau calea fișierului este obligatorie.");
        }

        // ✅ Modelele Whisper pe care le vom testa (fără "base")
        var whisperModels = new List<string> { "tiny", "small", "medium", "large" };
        var rezultateSimulare = new List<SimulareRezultat>();

        Console.WriteLine($"🚀 Începem simularea transcrierii pentru: {request.UrlOrPath}");

        // ✅ Descărcăm clipul audio dacă e URL
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

        // ✅ Creăm un folder unic pentru această sesiune
        string baseOutputDir = Path.Combine(Path.GetDirectoryName(audioPath), "transcrieri", Path.GetFileNameWithoutExtension(audioPath) + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(baseOutputDir);
        Console.WriteLine($"📁 Director de ieșire creat: {baseOutputDir}");

        // ✅ Transcriem cu fiecare model
        foreach (var model in whisperModels)
        {
            Console.WriteLine($"🎙️ Testăm modelul Whisper: {model}");

            var stopwatch = Stopwatch.StartNew();
            string modelOutputDir = Path.Combine(baseOutputDir, model);
            Directory.CreateDirectory(modelOutputDir);

            // ✅ Forțăm utilizarea CPU-ului
            string whisperCommand = $"python -m whisper \"{audioPath}\" --language {request.Language} --output_dir \"{modelOutputDir}\" --model {model} --output_format txt --fp16 False --device cpu";

            Console.WriteLine($"🔧 Comandă Whisper:\n{whisperCommand}");

            var processStartInfo = new ProcessStartInfo("cmd.exe", $"/C chcp 65001 && {whisperCommand}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // ✅ Setăm variabila de mediu pentru Python pentru a folosi UTF-8
            processStartInfo.Environment["PYTHONIOENCODING"] = "utf-8";


            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"📝 {e.Data}");
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine($"⚠️ [Eroare] {e.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // ✅ Loguri periodice pentru progres
                while (!process.HasExited)
                {
                    await Task.Delay(5000);  // Log la fiecare 5 secunde
                    Console.WriteLine($"⏳ Model '{model}' rulează încă... {stopwatch.Elapsed.TotalSeconds:F2}s");
                }

                await process.WaitForExitAsync();
            }

            stopwatch.Stop();

            // ✅ Verificăm dacă transcrierea a fost creată
            string transcriptionFile = Directory.GetFiles(modelOutputDir, "*.txt").FirstOrDefault();
            if (string.IsNullOrEmpty(transcriptionFile))
            {
                Console.WriteLine($"⚠️ Fișierul de transcriere lipsește pentru modelul {model}");
                continue;
            }

            string transcribedText = await System.IO.File.ReadAllTextAsync(transcriptionFile);

            // ✅ Simulăm un text de referință pentru calculul WER
            string referenceText = "This is the reference transcription text for accuracy comparison.";

            // ✅ Calculăm acuratețea (Word Error Rate)
            double wer = CalculateWER(referenceText, transcribedText);

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

    // ✅ Metodă pentru calculul Word Error Rate (WER)
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
                    distance[i - 1, j] + 1,       // ștergere
                    distance[i, j - 1] + 1,       // inserție
                    distance[i - 1, j - 1] + cost // substituție
                }.Min();
            }
        }

        double wer = (double)distance[refWords.Length, hypWords.Length] / refWords.Length;
        return Math.Max(0, Math.Min(wer, 1));  // ✅ Ne asigurăm că WER este între 0 și 1
    }
}
