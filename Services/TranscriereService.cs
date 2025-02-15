// TranscriereService.cs
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TranscriereYouTube.Interfaces;
using Xceed.Words.NET;

public class TranscriereService : ITranscriereService
{
    private readonly string _ffmpegPath;
    private readonly string _ytDlpPath;
    private readonly string _whisperPath;
    private readonly string _transcrieriFolder;

    public TranscriereService(IConfiguration configuration)
    {
        var settings = configuration.GetSection("TranscriereSettings");
        _ffmpegPath = settings["FFmpegPath"] ?? "ffmpeg";
        _ytDlpPath = settings["YT_DLPPath"] ?? "yt-dlp";
        _whisperPath = settings["WhisperPath"] ?? "whisper";
        _transcrieriFolder = Path.Combine(Directory.GetCurrentDirectory(), settings["TranscrieriFolder"] ?? "Transcrieri");
        Directory.CreateDirectory(_transcrieriFolder);
    }

    public string DescarcaVideo(string videoUrl)
    {
        var outputPath = Path.Combine(_transcrieriFolder, $"{Guid.NewGuid()}.mp4");
        var command = $"{_ytDlpPath} -f bestaudio -o \"{outputPath}\" {videoUrl}";
        ProcessRunner.Execute(command);
        return File.Exists(outputPath) ? outputPath : throw new Exception("Eroare la descărcare video.");
    }

    public string ExtrageAudio(string videoPath)
    {
        var audioPath = Path.Combine(_transcrieriFolder, $"{Guid.NewGuid()}.wav");
        var command = $"\"{_ffmpegPath}\" -i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{audioPath}\"";
        ProcessRunner.Execute(command);
        return File.Exists(audioPath) ? audioPath : throw new Exception("Eroare la extragerea audio.");
    }

    public async Task TranscrieAudioProgresiv(string audioPath, string limba, ConcurrentQueue<string> progres)
    {
        var command = $"\"{_whisperPath}\" \"{audioPath}\" --model large --language {limba} --output_format txt --output_dir \"{_transcrieriFolder}\"";
        await Task.Run(() => ProcessRunner.Execute(command, progres));
    }

    public async Task StartTranscriereAsync(string videoUrl, string limba)
    {
        var videoPath = DescarcaVideo(videoUrl);
        var audioPath = ExtrageAudio(videoPath);
        var progres = new ConcurrentQueue<string>();
        await TranscrieAudioProgresiv(audioPath, limba, progres);
    }

    public string GenereazaFisierTxt(string continut)
    {
        var path = Path.Combine(_transcrieriFolder, $"Transcriere_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        File.WriteAllText(path, continut);
        return path;
    }

    public string GenereazaFisierDocx(string continut)
    {
        var path = Path.Combine(_transcrieriFolder, $"Transcriere_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
        using var doc = DocX.Create(path);
        doc.InsertParagraph("📌 Transcriere Video").FontSize(18).Bold().Alignment = Xceed.Document.NET.Alignment.center;
        foreach (var line in continut.Split('\n'))
            doc.InsertParagraph(line).FontSize(12).SpacingAfter(5);
        doc.Save();
        return path;
    }

    public string GenereazaFisierPdf(string continut)
    {
        var path = Path.Combine(_transcrieriFolder, $"Transcriere_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        using var fs = new FileStream(path, FileMode.Create);
        var doc = new Document(PageSize.A4);
        PdfWriter.GetInstance(doc, fs);
        doc.Open();
        doc.Add(new Paragraph("📌 Transcriere Video") { Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16) });
        doc.Add(new Paragraph(continut) { Font = FontFactory.GetFont(FontFactory.HELVETICA, 12) });
        doc.Close();
        return path;
    }
}
