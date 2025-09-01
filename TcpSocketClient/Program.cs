// SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Net.Mail;

class TcpSocketClient {

    static string? LogFile;
    static  StreamWriter? LogWriter;
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

        //Check the args 
        if (args.Length != 4)
        {
            WriteLog(args.Length + " Arguments supplied.\nRequired: ip port Message|FilePath/message.txt messageCount");
            return;
        }

        string ip = args[0];
        string port = args[1];
        string message = args[2];
        if (message.Contains(".txt")) //treat as a file
            message = File.ReadAllText(message);
        int messageCount = Int32.Parse(args[3]);



        IPAddress localIpAddress = IPAddress.Parse(ip);
        IPEndPoint ipEndPoint = new(localIpAddress, Int32.Parse(port));
        using Socket client = new(
        ipEndPoint.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);

        //Logs
        LogFile = "log_" + ip + "-" + port + "_@" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
        LogWriter = File.AppendText(LogFile);

        try
        {
            WriteLog($"ConnectAsync: trying to connect on {ip}:{port}");
            await client.ConnectAsync(ipEndPoint);
            WriteLog("ConnectAsync: Connected");

            for (int i = 1; i <= messageCount; i++)
            {
                await SendMessage(client, message);
            }

            await client.DisconnectAsync(false);
            WriteLog("Socket client DisconnectAsync");

            client.Shutdown(SocketShutdown.Both);
            WriteLog("Socket client SocketShutdown");
        }
        catch (System.Exception exc)
        {
            WriteLog(exc.Message + Environment.NewLine + exc.StackTrace);
        }
        finally
        {
            LogWriter.Flush();
            LogWriter.Close();
            LogWriter.Dispose();

        }
    }

    static void WriteLog(string log) 
    {
        string msg = DateTime.Now.ToString("HH-mm-ss") + ": " + log;
        Console.WriteLine(msg);
        if(LogWriter != null)
            LogWriter.WriteLine(msg);

    }
    // Send message
    static async Task SendMessage(Socket client,string message)
    {
        WriteLog($"Socket client SendMessage: {message}");
        var messageBytes = Encoding.ASCII.GetBytes(message);

        WriteLog("Socket client SendMessage: Starting");
        _ = await client.SendAsync(messageBytes, SocketFlags.None);
        WriteLog("Socket client SendMessage: Done");

        // Receive ack.
        try
        {
            var buffer = new byte[1_024];
            WriteLog("Socket client ReceiveAsync: Starting");
            var received = await client.ReceiveAsync(buffer, SocketFlags.None);
            var response = Encoding.ASCII.GetString(buffer, 0, received);
            WriteLog($"Socket client ReceiveAsync Acknowledgment: \"{response}\"");

        }
        catch (System.Exception exc)
        {
            WriteLog("SendMessage: " + exc.Message + Environment.NewLine + exc.StackTrace);
            return;
        }   
     }
}
