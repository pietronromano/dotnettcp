// SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Net.Mail;

class TcpSocketClient {
    static async Task Main(string[] args)
    {

        var hostName = Dns.GetHostName();
        //IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
        // This is the IP address of the local machine
        //IPAddress localIpAddress = IPAddress.Any; //localhost.AddressList[3];

        string? ip = Environment.GetEnvironmentVariable("LISTENER_IP");
        ip ??= "127.0.0.1"; //set to default if null: SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0029-ide0030-ide0270

        IPAddress localIpAddress = IPAddress.Parse(ip);

        string? port = Environment.GetEnvironmentVariable("LISTENER_PORT");
        port ??= "11000"; //set to default if null: SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0029-ide0030-ide0270

        IPEndPoint ipEndPoint = new(localIpAddress, Int32.Parse(port));

        using Socket client = new(
        ipEndPoint.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);

        try
        {
            await client.ConnectAsync(ipEndPoint);

            for (int i = 1; i <= 10; i++)
            {
                await SendMessage(client, "Message " + i.ToString());
            }

            await client.DisconnectAsync(false);
            client.Shutdown(SocketShutdown.Both);
            Console.WriteLine("Shut Down");
        }
        catch (System.Exception exc)
        {
            Console.WriteLine(exc.Message + Environment.NewLine + exc.StackTrace);
            return;
        }
    }

    // Send message
    static async Task SendMessage(Socket client,string message)
    {
     
        var messageBytes = Encoding.UTF8.GetBytes(message);
        _ = await client.SendAsync(messageBytes, SocketFlags.None);
        Console.WriteLine("Socket client sent message");

        // Receive ack.
        try
        {
            var buffer = new byte[1_024];
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Socket client received acknowledgment: \"{response}\"");

        }
        catch (System.Exception exc)
        {
            Console.WriteLine("SendMessage: " + exc.Message + Environment.NewLine + exc.StackTrace);
            return;
        }   
     }
}
