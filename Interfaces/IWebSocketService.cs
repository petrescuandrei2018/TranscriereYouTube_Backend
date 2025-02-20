using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

public interface IWebSocketService
{
    Task TrimiteMesajAsync(WebSocket webSocket, string message, CancellationToken cancellationToken);
}
