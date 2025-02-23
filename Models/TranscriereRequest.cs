using System.Text.Json.Serialization;

namespace TranscriereYouTube_Backend.Models
{
    public class TranscriereRequest
    {
        public string UrlOrPath { get; set; }
        public string Language { get; set; } // Noua proprietate pentru limbă
    }

}
