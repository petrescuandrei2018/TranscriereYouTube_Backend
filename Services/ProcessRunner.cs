using System.Diagnostics;

public class ProcessRunner : IProcessRunner
{
    public async Task<Result<string>> RunCommandAsync(string fileName, string arguments)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName, // Executabilul (ex: yt-dlp.exe, ffmpeg.exe)
                Arguments = arguments, // Argumentele
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return Result<string>.Fail($"Eroare la executarea comenzii: {error}");
            }

            return Result<string>.Ok(output);
        }
        catch (Exception ex)
        {
            return Result<string>.Fail($"Excepție: {ex.Message}");
        }
    }
}
