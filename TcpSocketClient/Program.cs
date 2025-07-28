// SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Net.Mail;

class TcpSocketClient {
    static async Task Main(string[] args)
    {
        /* Local & Environment examples
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
        */

        if (args.Length != 4)
        {
            Console.WriteLine(args.Length + " Arguments supplied.\nRequired: ip port Message|FilePath/message.txt messageCount");
            return;
        }
        IPAddress localIpAddress = IPAddress.Parse(args[0]);
        string port = args[1];
        string message = args[2];
        if (message.Contains(".txt")) //treat as a file
            message = File.ReadAllText(message);
        int messageCount = Int32.Parse(args[3]);


        IPEndPoint ipEndPoint = new(localIpAddress, Int32.Parse(port));
        using Socket client = new(
        ipEndPoint.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);

        try
        {
            Console.WriteLine("ConnectAsync: Starting");
            await client.ConnectAsync(ipEndPoint);
            Console.WriteLine("ConnectAsync: Done");

            for (int i = 1; i <= messageCount; i++)
            {
                await SendMessage(client, message);
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
        Console.WriteLine("Socket client SendMessage: Starting");
        var messageBytes = Encoding.UTF8.GetBytes(message);
        _ = await client.SendAsync(messageBytes, SocketFlags.None);
        Console.WriteLine("Socket client SendMessage: Done");

        // Receive ack.
        try
        {
            var buffer = new byte[1_024];
            Console.WriteLine("Socket client ReceiveAsync: Starting");
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine($"Socket client ReceiveAsync Acknowledgment: \"{response}\"");

        }
        catch (System.Exception exc)
        {
            Console.WriteLine("SendMessage: " + exc.Message + Environment.NewLine + exc.StackTrace);
            return;
        }   
     }
}
