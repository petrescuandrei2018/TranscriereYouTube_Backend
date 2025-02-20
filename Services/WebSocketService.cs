using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class WebSocketService : IWebSocketService
{
    public async Task TrimiteMesajAsync(WebSocket webSocket, string message, CancellationToken cancellationToken)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(bytes);
        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }
}
