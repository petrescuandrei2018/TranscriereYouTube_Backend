using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace TranscriereYouTube.Utils
{
    public static class ProcessRunner
    {
        private static IConfiguration _config;

        // Inițializare configurație
        public static void Initialize(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Rulează un executabil specificat din appsettings.json cu argumentele oferite.
        /// </summary>
        public static (bool Success, string Output, string Error) Execute(string executableKey, string arguments)
        {
            try
            {
                Console.WriteLine($"[INFO] Pornire comandă: {executableKey} cu argumente: {arguments}");
                var executablePath = _config[$"TranscriereSettings:{executableKey}"];
                if (string.IsNullOrEmpty(executablePath))
                    throw new Exception($"Calea pentru {executableKey} nu este specificată în appsettings.json.");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = executablePath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                bool success = process.ExitCode == 0;

                if (!success)
                    Console.WriteLine($"[ERROR] Comanda a eșuat: {error}");
                else
                    Console.WriteLine($"[SUCCESS] Rezultat: {output}");

                return (success, output, error);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] Eroare la execuția comenzii: {ex.Message}");
                return (false, string.Empty, ex.Message);
            }
        }
    }
}
