using System.Collections.Concurrent;
using System.Threading.Tasks;

public interface ITranscriereService
{
    /// <summary>
    /// Inițiază procesul complet de transcriere: descărcare video, extragere audio și transcriere.
    /// </summary>
    Task StartTranscriereAsync(string videoUrl, string limba);

    /// <summary>
    /// Descarcă videoclipul de pe URL-ul specificat.
    /// </summary>
    string DescarcaVideo(string videoUrl);

    /// <summary>
    /// Extragere audio din fișierul video.
    /// </summary>
    string ExtrageAudio(string videoPath);

    /// <summary>
    /// Transcrierea audio cu progres trimis într-o coadă.
    /// </summary>
    Task TranscrieAudioProgresiv(string audioPath, string limba, ConcurrentQueue<string> progres);

    /// <summary>
    /// Transcriere simplă a fișierului audio.
    /// </summary>
    Task<string> TranscrieAudio(string audioPath, string limba);
}
