// SOURCE: https://www.c-sharpcorner.com/article/building-a-simple-socket-listener-in-net-core/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    public static void Main(string[] args)
    {
        // Start the socket listener in a background task
        Task listenerTask = StartListener();
        Console.ReadLine(); //Needed, or program just exits

    }

    private static async Task StartListener()
    {
        // Define the local endpoint for the socket
        IPAddress ipAddress = IPAddress.Any;

        string? port =  Environment.GetEnvironmentVariable("LISTENER_PORT");
        port ??= "11000"; //set to default if null: SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0029-ide0030-ide0270

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Int32.Parse(port));

        // Create a TCP/IP socket
        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            // Bind the socket to the local endpoint and listen for incoming connections
            listener.Bind(localEndPoint);
            listener.Listen(10);


            Console.WriteLine($"Waiting for a connection on port {port} ...");

            while (true)
            {

                // Start an asynchronous socket to listen for connections
                Socket handler = await listener.AcceptAsync();
                HandleConnection(handler);
                // Create a task to handle the connection
                //await Task.Run(() => HandleConnectionAsync(handler));
            }

            //Console.WriteLine("Shutting Down");
            //handler.Shutdown(SocketShutdown.Both);
            //handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static void HandleConnection(Socket handler)
    {
        // Buffer for incoming data
        byte[] buffer = new byte[1024];
        string data = "";

        try
        {
            // An incoming connection needs to be processed
            while (true)
            {
                int bytesRec =  handler.Receive(buffer, SocketFlags.None);
                if (bytesRec <= 0)
                {
                    Console.WriteLine("bytesRec <= 0. Breaking");
                    break;
                }

                // Convert the byte array to a string
                data = Encoding.ASCII.GetString(buffer, 0, bytesRec) + Environment.NewLine;

                // Check for end-of-file tag. If it is not there, read more data
                if (data.IndexOf("<EOM>") > -1)
                {
                    Console.WriteLine("<EOM> Found. Breaking");
                    break;
                }
                
            }

            // Show the data on the console
            Console.WriteLine(DateTime.UtcNow.ToString() + ": Text received : {0}", data);

            // Echo the data back to the client
            byte[] msg = Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString() + ": " + data);
             handler.Send(msg, SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    private static async Task HandleConnectionAsync(Socket handler)
    {
        // Buffer for incoming data
        byte[] buffer = new byte[1024];
        string data = "";

        try
        {
            // An incoming connection needs to be processed
            while (true)
            {
                int bytesRec = await handler.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesRec <= 0)
                {
                    Console.WriteLine("bytesRec <= 0. Breaking");
                    break;
                }

                // Convert the byte array to a string
                data = Encoding.ASCII.GetString(buffer, 0, bytesRec) + Environment.NewLine;

                // Check for end-of-file tag. If it is not there, read more data
                if (data.IndexOf("<EOM>") > -1)
                {
                    Console.WriteLine("<EOM> Found. Breaking");
                    break;
                }
                
            }

            // Show the data on the console
            Console.WriteLine(DateTime.UtcNow.ToString() + ": Text received : {0}", data);

            // Echo the data back to the client
            byte[] msg = Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString() + ": " + data);
            await handler.SendAsync(msg, SocketFlags.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }


}