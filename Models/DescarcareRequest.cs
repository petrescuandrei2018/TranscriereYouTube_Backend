using System;
using System.Diagnostics;

public static class ProcessRunner
{
    public static void Execute(string command)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", $"/c {command}")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            while (!process.StandardOutput.EndOfStream)
                Console.WriteLine($"[INFO] {process.StandardOutput.ReadLine()}");

            while (!process.StandardError.EndOfStream)
                Console.WriteLine($"[ERROR] {process.StandardError.ReadLine()}");

            process.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITIC] Eroare la execuția comenzii: {ex.Message}");
        }
    }
}
