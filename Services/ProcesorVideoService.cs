using TranscriereYouTube.Interfaces;
using System;

namespace TranscriereYouTube.Services
{
    public class ProcesorVideoService : IProcesorVideoService
    {
        public void CombinaVideoAudio(string videoPath, string audioPath)
        {
            Console.WriteLine("Combinare video + audio..."); 
        }
    }
}