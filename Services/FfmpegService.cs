public class FFmpegService : IFFmpegService
{
    private readonly IProcessRunner _processRunner;

    public FFmpegService(IProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<Result<string>> ExtractAudioAsync(string videoPath, string audioOutputPath)
    {
        string ffmpegPath = @"C:\FFmpeg\bin\ffmpeg.exe";
        string arguments = $"-y -i \"{videoPath}\" -vn -q:a 0 -map a \"{audioOutputPath}\"";

        // ✅ Logare comandă ffmpeg
        Console.WriteLine($"🔧 Comandă ffmpeg: {ffmpegPath} {arguments}");

        var result = await _processRunner.RunCommandAsync(ffmpegPath, arguments, "Extragere audio");

        if (!result.Success || !File.Exists(audioOutputPath))
        {
            var errorMessage = result.Success ? "⚠️ Fișierul audio nu a fost creat." : result.ErrorMessage;
            Console.WriteLine($"❌ Eroare la extragerea audio: {errorMessage}");
            return Result<string>.Fail(errorMessage);
        }

        Console.WriteLine($"✅ Audio extras cu succes: {audioOutputPath}");
        return Result<string>.Ok(audioOutputPath);
    }

    public async Task<Result<string>> ConvertVideoAsync(string inputPath, string outputPath)
    {
        string arguments = $"-i \"{inputPath}\" -c:v libx264 -preset fast -crf 23 -c:a copy \"{outputPath}\"";

        // ✅ Adaugă log pentru comandă ffmpeg (conversie video)
        Console.WriteLine($"🔧 Comandă ffmpeg (video): ffmpeg {arguments}");

        var result = await _processRunner.RunCommandAsync("ffmpeg", arguments, "Conversie video");

        if (!result.Success || !File.Exists(outputPath))
        {
            var errorMessage = result.Success ? "⚠️ Fișierul video convertit nu a fost creat." : result.ErrorMessage;
            Console.WriteLine($"❌ Eroare la conversia video: {errorMessage}");
            return Result<string>.Fail(errorMessage);
        }

        Console.WriteLine($"✅ Conversie video completă: {outputPath}");
        return Result<string>.Ok(outputPath);
    }

}
