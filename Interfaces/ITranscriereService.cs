using System.Collections.Concurrent;

public interface ITranscriereService
{
    string DescarcaVideo(string videoUrl);
    string ExtrageAudio(string videoPath);
    string TranscrieAudio(string audioPath, string limba);
    string GenereazaFisierDocx(string continut, string limba);
    string GenereazaFisierPdf(string continut, string limba);
    string GenereazaFisierTxt(string continut);
    void TranscrieAudioProgresiv(string audioPath, string limba, ConcurrentQueue<string> progres);
}
