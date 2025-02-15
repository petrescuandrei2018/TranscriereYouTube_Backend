using System;
using System.Diagnostics;

namespace TranscriereYouTube.Utils
{
    public class ProcessRunner
    {
        public static int RunCommand(string command)
        {
            try
            {
                var processInfo = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return -1;

                while (!process.StandardOutput.EndOfStream)
                {
                    var output = process.StandardOutput.ReadLine();
                    Console.WriteLine($"[INFO] {output}");
                }

                while (!process.StandardError.EndOfStream)
                {
                    var error = process.StandardError.ReadLine();
                    Console.WriteLine($"[ERROR] {error}");
                }

                process.WaitForExit();
                return process.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EROARE] Execuție comandă eșuată: {ex.Message}");
                return -1;
            }
        }
    }
}
