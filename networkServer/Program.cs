using System.Net;
using System.Net.Sockets;
using System.Text;

IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());
IPAddress ipAddress = ipHostInfo.AddressList[0];
IPEndPoint ipEndPoint = new(ipAddress, 8080);

using Socket listener = new(
    ipEndPoint.AddressFamily,
    SocketType.Stream,
    ProtocolType.Tcp);

listener.Bind(ipEndPoint);
listener.Listen(100);

var handler = await listener.AcceptAsync();

var rcvResp = "";
while (true)
{
    //receive message
    var rcvBuff = new byte[1024];
    var rcv = await handler.ReceiveAsync(rcvBuff, SocketFlags.None);
    rcvResp += Encoding.UTF8.GetString(rcvBuff);

    var EOM = "<|EOM|>";
    if (rcvResp.IndexOf(EOM) > -1)
    {
        Console.WriteLine($"Socket server received message: \"{rcvResp.Replace(EOM, "")}\"");

        var ACK = "<|ACK|>";
        var ACKBytes = Encoding.UTF8.GetBytes(ACK);
        await handler.SendAsync(ACKBytes, 0);
        Console.WriteLine($"Socket server sent acknowledgment: \"{ACK}\"");

        break;
    }
}

Console.WriteLine("\nPress enter to continue...\n");
while (Console.ReadKey().Key != ConsoleKey.Enter) { }
