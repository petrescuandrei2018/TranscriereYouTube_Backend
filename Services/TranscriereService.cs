using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ShellProgressBar;
using Xceed.Words.NET;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TranscriereYouTube.Interfaces;

// Alias-uri pentru a evita conflictele dintre Xceed și iTextSharp
using XceedParagraph = Xceed.Document.NET.Paragraph;
using XceedDocument = Xceed.Document.NET.Document;
using XceedFont = Xceed.Document.NET.Font;

using iTextParagraph = iTextSharp.text.Paragraph;
using iTextDocument = iTextSharp.text.Document;
using iTextFont = iTextSharp.text.Font;
using Xceed.Document.NET;
using System.Collections.Concurrent;

public class TranscriereService : ITranscriereService
{
    private readonly string _ffmpegPath = "ffmpeg";
    private readonly string _ytDlpPath = "yt-dlp";
    private readonly string _whisperPath = "whisper";

    public string DescarcaVideo(string videoUrl)
    {
        string outputPath = Path.Combine(Path.GetTempPath(), "video.mp4");
        string command = $"{_ytDlpPath} -f bestaudio -o \"{outputPath}\" {videoUrl}";

        RunCommand(command);
        AfiseazaDimensiuneFisier(outputPath, "📥 Video descărcat");

        return File.Exists(outputPath) ? outputPath : null;
    }

    public string ExtrageAudio(string videoPath)
    {
        string audioPath = Path.Combine(Path.GetTempPath(), "audio.wav");
        string command = $"\"{_ffmpegPath}\" -i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{audioPath}\"";

        RunCommand(command);
        AfiseazaDimensiuneFisier(audioPath, "🎵 Audio extras");

        return File.Exists(audioPath) ? audioPath : null;
    }

    public string TranscrieAudio(string audioPath, string limba)
    {
        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Transcrieri");
        Directory.CreateDirectory(outputDir);

        string outputPath = Path.Combine(outputDir, $"transcriere_{DateTime.Now:yyyyMMddHHmmss}.txt");
        string command = $"\"{_whisperPath}\" \"{audioPath}\" --model large --language {limba} --output_format txt --output_dir \"{outputDir}\"";

        int totalSteps = 100;
        using (var progressBar = new ProgressBar(totalSteps, "🔄 Transcriere în curs..."))
        {
            for (int i = 0; i <= totalSteps; i++)
            {
                Thread.Sleep(300);
                progressBar.Tick($"Progres: {i}%");
            }
        }

        RunCommand(command);
        return File.Exists(outputPath) ? outputPath : null;
    }

    public string GenereazaFisierDocx(string continut, string limba)
    {
        string path = $"C:\\Users\\And\\Desktop\\Transcriere_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
        using (var doc = DocX.Create(path))
        {
            doc.InsertParagraph("📌 Transcriere Video")
                .FontSize(18).Bold().SpacingAfter(10).Alignment = Alignment.center;

            var paragrafe = continut.Split(new[] { ". " }, StringSplitOptions.None);
            foreach (var par in paragrafe)
            {
                doc.InsertParagraph(par).FontSize(12).SpacingAfter(5);
            }

            doc.Save();
        }
        return path;
    }

    public string GenereazaFisierPdf(string continut, string limba)
    {
        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Transcrieri");
        Directory.CreateDirectory(outputDir);

        string filePath = Path.Combine(outputDir, $"Transcriere_{DateTime.Now:yyyyMMddHHmmss}.pdf");

        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            iTextDocument doc = new iTextDocument(PageSize.A4);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);
            doc.Open();

            iTextFont titlu = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            iTextFont normal = FontFactory.GetFont(FontFactory.HELVETICA, 12);

            doc.Add(new iTextParagraph("Transcriere Audio", titlu) { Alignment = Element.ALIGN_CENTER });
            doc.Add(new iTextParagraph($"Limbă: {limba}", normal));
            doc.Add(new iTextParagraph(" "));

            var paragrafe = continut.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var par in paragrafe)
            {
                doc.Add(new iTextParagraph(par.Trim() + ".", normal) { SpacingAfter = 5 });
            }

            doc.Close();
        }

        return filePath;
    }

    public string GenereazaFisierTxt(string continut)
    {
        string path = $"C:\\Users\\And\\Desktop\\Transcriere_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        File.WriteAllText(path, continut);
        return path;
    }


    public void TranscrieAudioProgresiv(string audioPath, string limba, ConcurrentQueue<string> progres)
    {
        string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Transcrieri");
        Directory.CreateDirectory(outputDir);

        string outputPath = Path.Combine(outputDir, $"transcriere_{DateTime.Now:yyyyMMddHHmmss}.txt");
        string command = $"\"{_whisperPath}\" \"{audioPath}\" --model medium --language {limba} --output_format txt --output_dir \"{outputDir}\"";

        progres.Enqueue("🔄 Începerea transcrierii...");

        // Proces async pentru execuția comenzii fără a bloca aplicația
        Task.Run(() =>
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.OutputDataReceived += (sender, args) => progres.Enqueue(args.Data);
                process.ErrorDataReceived += (sender, args) => progres.Enqueue($"❌ Eroare: {args.Data}");

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
            }

            progres.Enqueue("✅ Transcriere finalizată!");
        });
    }


    private void RunCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", $"/c {command}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
            }
        }

    private void AfiseazaDimensiuneFisier(string filePath, string mesaj)
    {
        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            Console.WriteLine($"{mesaj}: {fileInfo.Length / (1024 * 1024)} MB");
        }
    }

    public void AfiseazaProgres(int totalEtape, string mesaj)
    {
        var options = new ProgressBarOptions
        {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGray,
            ProgressCharacter = '█'
        };

        using (var progressBar = new ProgressBar(totalEtape, mesaj, options))
        {
            for (int i = 0; i < totalEtape; i++)
            {
                Thread.Sleep(1000); // Simulează progresul
                progressBar.Tick($"Pas {i + 1} din {totalEtape}");
            }
        }
    }
}
