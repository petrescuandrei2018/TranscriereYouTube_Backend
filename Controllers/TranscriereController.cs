using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
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
                return BadRequest("⚠️ URL-ul videoclipului și limba sunt necesare.");

            // ✅ Verificăm dacă limba este suportată
            var limbiSuportate = new HashSet<string> { "ro", "en" };
            if (!limbiSuportate.Contains(request.Limba.ToLower()))
            {
                return BadRequest($"❌ Limba specificată '{request.Limba}' nu este suportată. Limbile disponibile sunt: ro (română), en (engleză).");
            }

            // Inițiem transcrierea
            await _transcriereService.StartTranscriereAsync(request.VideoUrl, request.Limba);
            return Ok("🔹 Transcrierea a fost inițiată și va progresa în fundal.");
        }


        [HttpGet("progress")]
        public async Task GetProgress([FromQuery] string audioPath, [FromQuery] string limba, CancellationToken cancellationToken)
        {
            var context = HttpContext;

            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var progres = new ConcurrentQueue<string>();

                // ✅ Trimitere mesaj inițial
                await SendWebSocketMessage(webSocket, "🚀 Conexiune WebSocket stabilită. Începem transcrierea...");

                // ✅ Validare limbă
                var limbiSuportate = new HashSet<string> { "ro", "en" };
                if (string.IsNullOrEmpty(limba) || !limbiSuportate.Contains(limba.ToLower()))
                {
                    await SendWebSocketMessage(webSocket, $"❌ Limba specificată '{limba}' nu este suportată. Limbile disponibile sunt: ro (română), en (engleză).");
                    await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Limba neacceptată", cancellationToken);
                    return;
                }

                try
                {
                    // ✅ Trimitere mesaj de progres
                    await SendWebSocketMessage(webSocket, "🟢 Transcriere în curs...");

                    // ✅ Pornim transcrierea
                    await _transcriereService.TranscrieAudioProgresiv(audioPath, limba, progres);

                    // ✅ Trimiterea mesajelor din coada de progres
                    while (!cancellationToken.IsCancellationRequested || !progres.IsEmpty)
                    {
                        while (progres.TryDequeue(out var mesaj))
                        {
                            await SendWebSocketMessage(webSocket, $"🔔 {mesaj}");
                        }

                        await Task.Delay(500, cancellationToken);
                    }

                    // ✅ Transcriere finalizată
                    await SendWebSocketMessage(webSocket, "✅ Transcriere finalizată cu succes.");

                    // ✅ Închiderea conexiunii WebSocket
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Transcriere completă", cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await SendWebSocketMessage(webSocket, "⚠️ Transcriere anulată de utilizator.");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Transcriere anulată", cancellationToken);
                }
                catch (Exception ex)
                {
                    await SendWebSocketMessage(webSocket, $"❌ Eroare: {ex.Message}");
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Eroare în timpul transcrierii", cancellationToken);
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("⚠️ Cererea nu este un WebSocket valid.");
            }
        }

        [HttpPost("full-transcriere")]
        public async Task<IActionResult> FullTranscriere([FromBody] TranscriereRequest request)
        {
            if (string.IsNullOrEmpty(request.VideoUrl) || string.IsNullOrEmpty(request.Limba))
                return BadRequest("⚠️ URL-ul videoclipului și limba sunt necesare.");

            var limbiSuportate = new HashSet<string> { "ro", "en" };
            if (!limbiSuportate.Contains(request.Limba.ToLower()))
                return BadRequest($"❌ Limba '{request.Limba}' nu este suportată. Limbile disponibile sunt: ro (română), en (engleză).");

            try
            {
                // ✅ Pasul 1: Descărcăm videoclipul
                var videoPath = _transcriereService.DescarcaVideo(request.VideoUrl);
                if (string.IsNullOrEmpty(videoPath))
                    return StatusCode(500, "❌ Eroare la descărcarea videoclipului.");

                // ✅ Pasul 2: Extragerea audio
                var audioPath = _transcriereService.ExtrageAudio(videoPath);
                if (string.IsNullOrEmpty(audioPath))
                    return StatusCode(500, "❌ Eroare la extragerea audio.");

                // ✅ Pasul 3: Transcrierea audio
                var progres = new ConcurrentQueue<string>();
                await _transcriereService.TranscrieAudioProgresiv(audioPath, request.Limba, progres);

                // ✅ Rezultatul final
                var transcriere = string.Join(Environment.NewLine, progres.ToArray());

                return Ok(new
                {
                    Mesaj = "✅ Transcriere finalizată cu succes.",
                    Transcriere = transcriere,
                    VideoPath = videoPath,
                    AudioPath = audioPath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"❌ Eroare în timpul procesului: {ex.Message}");
            }
        }

        private async Task SendWebSocketMessage(WebSocket webSocket, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var buffer = new ArraySegment<byte>(bytes);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }

    }
}
