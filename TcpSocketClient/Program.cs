// SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Net.Mail;

class TcpSocketClient {

    
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
        if (args.Length != 5)
        {
            WriteLog(args.Length + " Arguments supplied.\nRequired: ip port Message|FilePath/message.txt messageCount timeout");
            return;
        }

        string ip = args[0];
        string port = args[1];
        string message = args[2];
        
        int messageCount = 1;
        if (!Int32.TryParse(args[3], out messageCount))
        {
            Console.WriteLine(args[3] + " is not an integer. Defaulting to message count of 1");
            messageCount = 1;
        }
        
        int timeout = 10000;
        if (!Int32.TryParse(args[4], out timeout))
        {
            Console.WriteLine(args[4] + " is not an integer. Defaulting to timeout of 10 seconds");
            timeout = 10000;
        }

        try
        {
            if (message.Contains(".txt")) //treat as a file
            {
                string messagePath = Path.Join(Environment.CurrentDirectory, message);
                message = File.ReadAllText(messagePath);

            }

        }
        catch (System.Exception exc)
        {
            Console.WriteLine("Error reading message file .txt: " + exc.Message);
            return;
        }
           

        IPAddress localIpAddress = IPAddress.Parse(ip);
        IPEndPoint ipEndPoint = new(localIpAddress, Int32.Parse(port));
        using Socket client = new(
        ipEndPoint.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);
        
        //Logs
        string logFile = "log_" + DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss") + "@" + ip + "-" + port  + ".txt";
        string logPath = Path.Join(Environment.CurrentDirectory, "logs");
        if (!Path.Exists(logPath))
            System.IO.Directory.CreateDirectory(logPath);

        logPath = Path.Join(logPath,logFile);
        LogWriter = File.AppendText(logPath);

        try
        {
            WriteLog($"Connect: trying to connect on {ip}:{port}");

            if (!client.ConnectAsync(ipEndPoint).Wait(timeout))
            {
                WriteLog($"Connect timed out after {timeout} milliseconds");
                throw new Exception($"Connection timeout");
            }
            WriteLog("Connect: Connected");

            for (int i = 1; i <= messageCount; i++)
            {
                SendMessage(client, message, timeout);
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
        string msg = DateTime.Now.ToString("HH:mm:ss") + ": " + log;
        Console.WriteLine(msg);
        if(LogWriter != null)
            LogWriter.WriteLine(msg);

    }
    // Send message
    static void SendMessage(Socket client,string message, int timeout)
    {
        WriteLog($"Socket client SendMessage: {message}");
        var messageBytes = Encoding.ASCII.GetBytes(message);

        WriteLog("Socket client SendAsync: Starting");
        if(!client.SendAsync(messageBytes, SocketFlags.None).Wait(timeout))
        {
            WriteLog($"SendAsync timed out after {timeout} milliseconds");
            throw new Exception($"SendAsync timeout");
        }

        WriteLog("Socket client SendMessage: Done");

        // Receive ack.
        try
        {
            var buffer = new byte[1_024];
            WriteLog("Socket client ReceiveAsync: Starting");
            if(!client.ReceiveAsync(buffer, SocketFlags.None).Wait(timeout))
            {
                WriteLog($"ReceiveAsync timed out after {timeout} milliseconds");
                throw new Exception($"ReceiveAsync timeout");
            }
            var response = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            WriteLog($"Socket client ReceiveAsync Acknowledgment: \"{response}\"");

        }
        catch (System.Exception exc)
        {
            WriteLog("SendMessage: " + exc.Message + Environment.NewLine + exc.StackTrace);
            return;
        }   
     }
}
