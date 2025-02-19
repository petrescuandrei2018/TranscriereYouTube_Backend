using System;
using System.Diagnostics;
using System.IO;
using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Utils;

public class ProcesorVideoService : IProcesorVideoService
{
    public void CombinaVideoAudio(string videoPath, string audioPath)
    {
        // Construim argumentele pentru ffmpeg
        string arguments = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -strict experimental \"video_final.mp4\"";

        // Folosim cheia "FFmpegPath" din appsettings.json
        var (success, output, error) = ProcessRunner.Execute("FFmpegPath", arguments);

        if (!success)
        {
            Console.WriteLine($"Eroare la combinarea video și audio: {error}");
            throw new Exception("Combinarea video și audio a eșuat.");
        }
        else
        {
            Console.WriteLine("Combinarea video și audio s-a realizat cu succes.");
        }
    }

    public bool VerificaIntegritateFisier(string filePath)
    {
        return File.Exists(filePath) && new FileInfo(filePath).Length > 0;
    }
}
