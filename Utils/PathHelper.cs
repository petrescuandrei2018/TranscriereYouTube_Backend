using System.IO;

namespace TranscriereYouTube_Backend.Utils
{
    public static class PathHelper
    {
        /// <summary>
        /// Combină mai multe căi într-o singură cale completă.
        /// </summary>
        public static string CombinaCale(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Verifică dacă un director există și îl creează dacă lipsește.
        /// </summary>
        public static void CreeazaDirector(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Returnează extensia fișierului.
        /// </summary>
        public static string ObtineExtensie(string filePath)
        {
            return Path.GetExtension(filePath);
        }
    }
}
