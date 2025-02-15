using System;
using System.Diagnostics;
using System.IO;
using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Utils;

public class ProcesorVideoService : IProcesorVideoService
{
    public void CombinaVideoAudio(string videoPath, string audioPath)
    {
        string command = $"ffmpeg -i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -strict experimental \"video_final.mp4\"";
        ProcessRunner.Execute(command);
    }

    public bool VerificaIntegritateFisier(string filePath)
    {
        return File.Exists(filePath) && new FileInfo(filePath).Length > 0;
    }
}
