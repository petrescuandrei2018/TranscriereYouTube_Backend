using System.IO;

namespace TranscriereYouTube_Backend.Utils
{
    public static class FileHelper
    {
        /// <summary>
        /// Verifică dacă un fișier există.
        /// </summary>
        public static bool ExistaFisier(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Scrie conținut într-un fișier.
        /// </summary>
        public static void ScrieFisier(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Citește conținutul unui fișier.
        /// </summary>
        public static string CitesteFisier(string path)
        {
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        /// <summary>
        /// Șterge fișierul specificat.
        /// </summary>
        public static void StergeFisier(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
