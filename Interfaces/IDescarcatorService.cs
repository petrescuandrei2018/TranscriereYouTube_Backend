namespace TranscriereYouTube.Interfaces
{
    public interface IDescarcatorService
    {
        string Descarca(string videoUrl);
        string ExtrageAudio(string videoPath);
    }
}
