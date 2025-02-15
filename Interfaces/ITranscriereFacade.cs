namespace TranscriereYouTube.Interfaces
{
    public interface ITranscriereFacade
    {
        /// <summary>
        /// Inițiază procesul de transcriere pentru fișierul audio specificat.
        /// </summary>
        /// <param name="audioPath">Calea către fișierul audio.</param>
        /// <param name="limba">Limba utilizată pentru transcriere (ex. "ro", "en").</param>
        /// <returns>Textul transcris sau mesaj de eroare în caz de eșec.</returns>
        string Transcrie(string audioPath, string limba);
    }
}
