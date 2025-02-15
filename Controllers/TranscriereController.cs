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
                return BadRequest("URL-ul videoclipului și limba sunt necesare.");

            await _transcriereService.StartTranscriereAsync(request.VideoUrl, request.Limba);
            return Ok("🔹 Transcrierea a fost inițiată și va progresa în fundal.");
        }

        [HttpGet("progress")]
        public async Task GetProgress([FromQuery] string audioPath, CancellationToken cancellationToken)
        {
            var context = HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var progres = new ConcurrentQueue<string>();

                await _transcriereService.TranscrieAudioProgresiv(audioPath, "ro", progres);

                while (!cancellationToken.IsCancellationRequested)
                {
                    while (progres.TryDequeue(out var mesaj))
                    {
                        var bytes = Encoding.UTF8.GetBytes(mesaj);
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
                    }
                    await Task.Delay(500);
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}
