using TranscriereYouTube.Utils;

public class DescarcatorService : IDescarcatorService
{
    public string Descarca(string videoUrl)
    {
        var outputPath = $"videoclipuri/{Guid.NewGuid()}.mp4";

        // Argumentele pentru yt-dlp
        var arguments = $"-f bestvideo+bestaudio -o \"{outputPath}\" \"{videoUrl}\"";

        // Folosește cheia "YT_DLPPath" din appsettings.json
        var (success, output, error) = ProcessRunner.Execute("YT_DLPPath", arguments);

        if (!success)
        {
            Console.WriteLine($"Eroare la descărcare: {error}");
            throw new Exception("Descărcarea videoclipului a eșuat.");
        }

        return outputPath;
    }
}
