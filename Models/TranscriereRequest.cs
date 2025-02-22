namespace TranscriereYouTube_Backend.Models
{
    public class TranscriereRequest
    {
        public string UrlOrPath { get; set; }  // URL YouTube sau cale locală
        public string Language { get; set; }   // Limbă pentru transcriere
    }
}
