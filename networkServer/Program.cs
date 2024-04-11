using System.Net.Sockets;
using System.Text;

namespace networkServer
{
    public partial class Program
    { 
        public static async Task Main(string[] args)
        {

            var listener = await CreateListenerSocket("localhost", 8080);
            var handler = await listener.AcceptAsync();
            string received = await ReceiveFromSocket(handler);
            Message? message = ParseJson(Encoding.UTF8.GetBytes(received));
            Console.WriteLine(message?.Data);

            Console.WriteLine("\nPress enter to continue...\n");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}