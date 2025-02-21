namespace TranscriereYouTube_Backend.Utils
{
    public class TestProcesExtern
    {
        private readonly IProcessRunner _processRunner;

        public TestProcesExtern()
        {
            _processRunner = new ProcessRunner();  // Asigură-te că ai implementarea corectă
        }

        // ✅ Test yt-dlp
        public async Task<Result<string>> TestYtDlp()
        {
            var ytDlpPath = @"C:\Python313\Scripts\yt-dlp.exe";
            var videoUrl = "https://www.youtube.com/watch?v=KBvQcbDEjlM";
            var outputPath = @"C:\Users\And\Desktop\C#\TranscriereYouTube_Backend\video.mp4";
            var arguments = $"-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{outputPath}\" \"{videoUrl}\"";

            return await _processRunner.RunCommandAsync(ytDlpPath, arguments);
        }

        // ✅ Test ffmpeg
        public async Task<Result<string>> TestFfmpeg()
        {
            var ffmpegPath = @"C:\FFmpeg\bin\ffmpeg.exe";
            var videoInput = @"C:\Users\And\Desktop\C#\TranscriereYouTube_Backend\video.mp4";
            var audioOutput = @"C:\Users\And\Desktop\C#\TranscriereYouTube_Backend\audio.mp3";
            var arguments = $"-i \"{videoInput}\" -q:a 0 -map a \"{audioOutput}\"";

            return await _processRunner.RunCommandAsync(ffmpegPath, arguments);
        }

        // ✅ Test whisper
        public async Task<Result<string>> TestWhisper()
        {
            var whisperPath = @"C:\Program Files\Python311\Scripts\whisper.exe";
            var audioPath = @"C:\Users\And\Desktop\C#\TranscriereYouTube_Backend\audio.mp3";
            var language = "en"; // sau "ro"
            var arguments = $"\"{audioPath}\" --language {language}";

            return await _processRunner.RunCommandAsync(whisperPath, arguments);
        }
    }
}
