using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using TranscriereYouTube.Interfaces;

namespace TranscriereYouTube.Controllers
{
    [ApiController]
    [Route("api/transcriere")]
    public class TranscriereController : ControllerBase
    {
        private readonly ITranscriereService _transcriereService;

        public TranscriereController(ITranscriereService transcriereService)
        {
            _transcriereService = transcriereService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartTranscriere([FromBody] TranscriereRequest request)
        {
            if (string.IsNullOrEmpty(request.VideoUrl) || string.IsNullOrEmpty(request.Limba))
                return BadRequest(new { Eroare = "URL-ul videoclipului și limba sunt necesare." });

            Response.Headers.Add("Content-Type", "text/event-stream");
            using (var writer = new StreamWriter(Response.Body))
            {
                await writer.WriteLineAsync("event: progres\n");
                await writer.WriteLineAsync("data: Descărcare video începută...\n\n");
                await writer.FlushAsync();

                var videoPath = _transcriereService.DescarcaVideo(request.VideoUrl);
                if (string.IsNullOrEmpty(videoPath))
                {
                    await writer.WriteLineAsync("data: Eroare la descărcare!\n\n");
                    await writer.FlushAsync();
                    return StatusCode(500);
                }

                await writer.WriteLineAsync("data: Extragere audio...\n\n");
                await writer.FlushAsync();

                var audioPath = _transcriereService.ExtrageAudio(videoPath);
                if (string.IsNullOrEmpty(audioPath))
                {
                    await writer.WriteLineAsync("data: Eroare la extragerea audio!\n\n");
                    await writer.FlushAsync();
                    return StatusCode(500);
                }

                await writer.WriteLineAsync("data: Transcriere audio în curs...\n\n");
                await writer.FlushAsync();

                var textTranscris = _transcriereService.TranscrieAudio(audioPath, request.Limba);
                if (string.IsNullOrEmpty(textTranscris))
                {
                    await writer.WriteLineAsync("data: Eroare la transcriere!\n\n");
                    await writer.FlushAsync();
                    return StatusCode(500);
                }

                await writer.WriteLineAsync("data: Transcriere finalizată!\n\n");
                await writer.FlushAsync();

                return Ok(new { Mesaj = "Transcriere finalizată!" });
            }
        }

        [HttpPost("stream-transcriere")]
        public async Task<IActionResult> StreamTranscriere([FromBody] TranscriereRequest request)
        {
            if (string.IsNullOrEmpty(request.VideoUrl) || string.IsNullOrEmpty(request.Limba))
               return BadRequest("URL-ul videoclipului și limba sunt necesare.");

            var stream = new ConcurrentQueue<string>(); // Coada pentru progres

            var task = Task.Run(() =>
            {
                // Pas 1: Descărcare video
                var videoPath = _transcriereService.DescarcaVideo(request.VideoUrl);
                stream.Enqueue("✅ Video descărcat...");

                // Pas 2: Extragere audio
                var audioPath = _transcriereService.ExtrageAudio(videoPath);
                stream.Enqueue("🎧 Audio extras...");

                // Pas 3: Transcriere audio în text progresiv
                _transcriereService.TranscrieAudioProgresiv(audioPath, request.Limba, stream);
                stream.Enqueue("✅ Transcriere finalizată!");
            });

            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");

            while (!task.IsCompleted || !stream.IsEmpty)
            {
                if (stream.TryDequeue(out var line))
                {
                    await Response.WriteAsync($"data: {line}\n\n");
                    await Response.Body.FlushAsync();
                }
                await Task.Delay(500);
            }

            return Ok("Transcriere completă!");
        }

        [HttpPost("genereaza-txt")]
        public IActionResult GenereazaFisierTxt([FromBody] TranscriereRequest request)
        {
            if (string.IsNullOrEmpty(request.VideoUrl) || string.IsNullOrEmpty(request.Limba))
                return BadRequest("URL-ul videoclipului și limba sunt necesare.");

            // Pasul 1: Descărcare video
            var videoPath = _transcriereService.DescarcaVideo(request.VideoUrl);
            if (string.IsNullOrEmpty(videoPath))
                return StatusCode(500, "Eroare la descărcarea videoclipului.");

            // Pasul 2: Extragere audio
            var audioPath = _transcriereService.ExtrageAudio(videoPath);
            if (string.IsNullOrEmpty(audioPath))
                return StatusCode(500, "Eroare la extragerea audio.");

            // Pasul 3: Transcriere audio în text
            var textTranscris = _transcriereService.TranscrieAudio(audioPath, request.Limba);
            if (string.IsNullOrEmpty(textTranscris))
                return StatusCode(500, "Eroare la transcriere.");

            // Pasul 4: Generare fișier TXT
            var fisierTxt = _transcriereService.GenereazaFisierTxt(textTranscris);

            return Ok(new { Mesaj = "Fișier TXT generat!", CaleFisier = fisierTxt });
        }


        [HttpGet("descarcare")]
        public IActionResult DescarcaFisier([FromQuery] string fisier)
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Transcrieri", fisier);

            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Eroare = "Fișierul nu există." });

            string contentType = "application/octet-stream";
            if (fisier.EndsWith(".pdf")) contentType = "application/pdf";
            if (fisier.EndsWith(".docx")) contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            return PhysicalFile(filePath, contentType, fisier);
        }


    }

    public class TranscriereRequest
    {
        public string VideoUrl { get; set; } = string.Empty;
        public string Limba { get; set; } = string.Empty; // "ro" sau "en"
    }
}
