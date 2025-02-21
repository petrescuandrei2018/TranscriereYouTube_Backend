using System.Diagnostics;

public class ProcessRunner : IProcessRunner
{
    public async Task<Result<string>> RunCommandAsync(string executable, string arguments, string taskDescription = "Proces în desfășurare")
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = processInfo };
        process.Start();

        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            return Result<string>.Fail($"Error: {error}");
        }

        return Result<string>.Ok(output);
    }

    public async Task<Result<string>> ConvertAv1ToH264Async(string inputPath)
    {
        string ffmpegPath = "ffmpeg";
        string outputPath = Path.Combine(Path.GetDirectoryName(inputPath), Path.GetFileNameWithoutExtension(inputPath) + "_converted.mp4");

        string arguments = $"-y -i \"{inputPath}\" -c:v libx264 -preset fast -crf 23 -c:a copy \"{outputPath}\"";

        var result = await RunCommandAsync(ffmpegPath, arguments, "Conversie AV1 -> H.264");

        return result.Success ? Result<string>.Ok(outputPath) : Result<string>.Fail(result.ErrorMessage);
    }
}
