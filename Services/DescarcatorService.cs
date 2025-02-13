using TranscriereYouTube.Interfaces;
using System.Diagnostics;

namespace TranscriereYouTube.Services
{
    public class DescarcatorService : IDescarcatorService
    {
        public string Descarca(string videoUrl)
        {
            var outputPath = $"videoclipuri/{Guid.NewGuid()}.mp4";
            var command = $"yt-dlp -f bestvideo+bestaudio -o {outputPath} {videoUrl}";

            RunCommand(command);
            return outputPath;
        }

        public string ExtrageAudio(string videoPath)
        {
            var audioPath = videoPath.Replace(".mp4", ".wav");
            var command = $"ffmpeg -i {videoPath} -vn -acodec pcm_s16le -ar 44100 -ac 2 {audioPath}";

            RunCommand(command);
            return audioPath;
        }

        private void RunCommand(string command)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
