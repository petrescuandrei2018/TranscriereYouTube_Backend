using iTextSharp.text.pdf.codec.wmf;
using System.Diagnostics;

public class ProcesorVideoService : IProcesorVideoService
{
    private readonly IProcessRunner _processRunner;
    private readonly ICommandFactory _commandFactory;

    public ProcesorVideoService(IProcessRunner processRunner, ICommandFactory commandFactory)
    {
        _processRunner = processRunner;
        _commandFactory = commandFactory;
    }

    public async Task<Result<string>> ConvertVideoFormatAsync(string inputPath, string outputFormat)
    {
        var outputPath = Path.ChangeExtension(inputPath, outputFormat);
        var command = $"ffmpeg -i \"{inputPath}\" \"{outputPath}\"";

        var result = await _processRunner.RunCommandAsync("cmd.exe", $"/C {command}");

        return result.Success ? Result<string>.Ok(outputPath) : Result<string>.Fail(result.ErrorMessage);
    }

    public async Task<Result<string>> ExtractClipAsync(string inputPath, TimeSpan startTime, TimeSpan duration)
    {
        var outputPath = $"{Path.GetFileNameWithoutExtension(inputPath)}_clip.mp4";
        var command = $"ffmpeg -i \"{inputPath}\" -ss {startTime} -t {duration} -c copy \"{outputPath}\"";

        var result = await _processRunner.RunCommandAsync("cmd.exe", $"/C {command}");

        return result.Success ? Result<string>.Ok(outputPath) : Result<string>.Fail(result.ErrorMessage);
    }
    
    // ✅ Metodă pentru a verifica dacă fișierul video este valid
    public bool EsteFisierVideoValid(string caleFisier)
    {
        if (!File.Exists(caleFisier))
        {
            Console.WriteLine("❌ Fișierul nu există.");
            return false;
        }

        try
        {
            // ✅ Folosim ffprobe (din ffmpeg) pentru a verifica integritatea fișierului
            var ffprobePath = @"C:\FFmpeg\bin\ffprobe.exe"; // Schimbă calea dacă este diferită
            var arguments = $"-v error -show_format -show_streams \"{caleFisier}\"";

            var processInfo = new ProcessStartInfo
            {
                FileName = ffprobePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string errorOutput = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0 || string.IsNullOrEmpty(output))
            {
                Console.WriteLine($"❌ Fișier invalid sau corupt. Eroare: {errorOutput}");
                return false;
            }

            Console.WriteLine("✅ Fișierul video este valid și poate fi folosit.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepție la verificarea fișierului: {ex.Message}");
            return false;
        }
    }
}
