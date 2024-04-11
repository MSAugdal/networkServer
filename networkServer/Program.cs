using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Application
{
    public class Message
    {
        public string? Data { get; set; }
        public string? Sender { get; set; }
        public DateTime? Time { get; set; }
    }

    public class Program
    {
        private static async Task<string> ReceiveFromSocket(Socket socket)
        {
            var EOM = "<|EOM|>";
            var rcvResp = "";
            while (true)
            {
                //receive message
                var rcvBuff = new byte[1024];
                await socket.ReceiveAsync(rcvBuff, SocketFlags.None);
                rcvResp += Encoding.UTF8.GetString(rcvBuff);

                if (rcvResp.IndexOf(EOM) > -1)
                {
                    Console.WriteLine($"Socket server received message: \"{rcvResp.Replace(EOM, "")}\"");

                    var ACK = "<|ACK|>";
                    var ACKBytes = Encoding.UTF8.GetBytes(ACK);
                    await socket.SendAsync(ACKBytes, 0);
                    Console.WriteLine($"Socket server sent acknowledgment: \"{ACK}\"");

                    break;
                }
            }
            return rcvResp.Replace(EOM, "");
        }

        private static async Task<Socket> CreateListenerSocket(string host, int port)
        {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint ipEndPoint = new(ipAddress, 8080);

            Socket listener = new(
                ipEndPoint.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(10);

            return listener;
        }

        private static Message? ParseJson(byte[] jsonUtf8Bytes)
        {
            if (jsonUtf8Bytes == null) { return new Message(); }

            Utf8JsonReader utf8Reader = new(jsonUtf8Bytes);
            Message? deserialized = JsonSerializer.Deserialize<Message>(ref utf8Reader);

            return deserialized;
        }

        public static async Task Main(string[] args)
        {
            var listener = await CreateListenerSocket("localhost", 8080);
            var handler = await listener.AcceptAsync();
            string received = await ReceiveFromSocket(listener);
            ParseJson(Encoding.UTF8.GetBytes(received));

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            Console.WriteLine("\nPress enter to continue...\n");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }
        }
    }
}