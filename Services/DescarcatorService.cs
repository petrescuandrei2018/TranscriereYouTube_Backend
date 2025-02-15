using System;
using System.Diagnostics;
using System.IO;
using TranscriereYouTube.Interfaces;
using TranscriereYouTube.Utils;

public class DescarcatorService : IDescarcatorService
{
    public string Descarca(string videoUrl)
    {
        var outputPath = $"videoclipuri/{Guid.NewGuid()}.mp4";
        var command = $"yt-dlp -f bestvideo+bestaudio -o \"{outputPath}\" \"{videoUrl}\"";

        ProcessRunner.Execute(command);
        return outputPath;
    }
}
