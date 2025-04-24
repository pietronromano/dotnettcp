// SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class TcpSocketClient {
    static async Task Main(string[] args)
    {

    var hostName = Dns.GetHostName();
    //IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
    // This is the IP address of the local machine
    //IPAddress localIpAddress = IPAddress.Any; //localhost.AddressList[3];

    string? ip =  Environment.GetEnvironmentVariable("LISTENER_IP");
    ip ??= "127.0.0.1"; //set to default if null: SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0029-ide0030-ide0270

    IPAddress localIpAddress = IPAddress.Parse(ip);

    string? port =  Environment.GetEnvironmentVariable("LISTENER_PORT");
    port ??= "11000"; //set to default if null: SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0029-ide0030-ide0270

    IPEndPoint ipEndPoint = new(localIpAddress, Int32.Parse(port));

    using Socket client = new(
    ipEndPoint.AddressFamily, 
    SocketType.Stream, 
    ProtocolType.Tcp);

    try
    {
        await client.ConnectAsync(ipEndPoint);
    }
    catch (System.Exception exc)
    {
        Console.WriteLine(exc.Message + Environment.NewLine + exc.StackTrace);
        return;
    }

//for (int i = 0; i < 10; i++)  looping didn't work
   
    // Send message.
    var message = "Message from TcpSocketClient <EOM>";
    var messageBytes = Encoding.UTF8.GetBytes(message);
    _ = await client.SendAsync(messageBytes, SocketFlags.None);
    Console.WriteLine($"Socket client sent message: \"{message}\"");

    // Receive ack.
    var buffer = new byte[1_024];
    var received = await client.ReceiveAsync(buffer, SocketFlags.None);
    var response = Encoding.UTF8.GetString(buffer, 0, received);
    Console.WriteLine($"Socket client received acknowledgment: \"{response}\"");
    
    await client.DisconnectAsync(false);

    //Console.WriteLine("Sleeping");
   // Thread.Sleep(1000);

    client.Shutdown(SocketShutdown.Both);

    }
}
