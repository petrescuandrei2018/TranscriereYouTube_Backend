namespace Utils
{
    public static class ConsoleHelper
    {
        /// <summary>
        /// Afișează un progress bar în consolă.
        /// </summary>
        public static void ShowProgressBar(string mesaj, int progres, int total)
        {
            int latimeBar = 50; // Lățimea progress bar-ului
            double procent = (double)progres / total;
            int latimeCurenta = (int)(procent * latimeBar);

            Console.Write($"\r{mesaj} [");
            Console.Write(new string('#', latimeCurenta));
            Console.Write(new string('-', latimeBar - latimeCurenta));
            Console.Write($"] {procent * 100:0.00}%");
        }
    }
}
