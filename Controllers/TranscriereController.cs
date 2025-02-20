using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

[ApiController]
[Route("api/transcriere")]
public class TranscriereController : ControllerBase
{
    private readonly ITranscriereFacade _transcriereFacade;
    private readonly ITranscriereValidator _validator;
    private readonly IWebSocketService _webSocketService;

    public TranscriereController(
        ITranscriereFacade transcriereFacade,
        ITranscriereValidator validator,
        IWebSocketService webSocketService)
    {
        _transcriereFacade = transcriereFacade;
        _validator = validator;
        _webSocketService = webSocketService;
    }

    [HttpPost("full-transcriere")]
    public async Task<IActionResult> FullTranscriere([FromBody] TranscriereRequest request)
    {
        Console.WriteLine("🚀 Începem transcrierea completă...");

        // ✅ Apelăm metoda din Facade
        var result = await _transcriereFacade.ExecuteFullTranscription(request.VideoPath, request.Language);

        if (!result.Success)
        {
            Console.WriteLine($"❌ Eroare: {result.ErrorMessage}");
            return BadRequest(new { eroare = result.ErrorMessage });
        }

        Console.WriteLine("✅ Transcriere completă!");
        return Ok(new { transcriere = result.Data });
    }


    [HttpGet("progress")]
    public async Task GetProgress([FromQuery] string audioPath, [FromQuery] string limba, CancellationToken cancellationToken)
    {
        var context = HttpContext;

        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            try
            {
                await _webSocketService.TrimiteMesajAsync(webSocket, "🚀 Începem transcrierea...", cancellationToken);
                var result = await _transcriereFacade.ExecuteFullTranscription(audioPath, limba);

                if (!result.Success)
                {
                    await _webSocketService.TrimiteMesajAsync(webSocket, $"❌ Eroare: {result.ErrorMessage}", cancellationToken);
                }
                else
                {
                    await _webSocketService.TrimiteMesajAsync(webSocket, "✅ Transcriere finalizată cu succes.", cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await _webSocketService.TrimiteMesajAsync(webSocket, $"❗ Eroare neprevăzută: {ex.Message}", cancellationToken);
            }
            finally
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Conexiune închisă", cancellationToken);
            }
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("⚠️ Cererea nu este un WebSocket valid.");
        }
    }

}
