using System.Collections.Concurrent;

public interface ITranscriereService
{
    Task StartTranscriereAsync(string videoUrl, string limba);
    string DescarcaVideo(string videoUrl);
    string ExtrageAudio(string videoPath);
    Task TranscrieAudioProgresiv(string audioPath, string limba, ConcurrentQueue<string> progres);
}
