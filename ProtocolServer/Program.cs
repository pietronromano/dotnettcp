using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ProtocolServer.Transport;

var listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

IPAddress ipAddress = IPAddress.Any;
string? port =  Environment.GetEnvironmentVariable("LISTENER_PORT");
port ??= "11000"; //set to default if null: SEE https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0029-ide0030-ide0270
IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Int32.Parse(port));

try
{
    listenSocket.Bind(localEndPoint); //listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 8989));
    listenSocket.Listen(128);
    Console.WriteLine($"Waiting for a connection on port {port} ...");
}
catch (System.Exception exc)
{
    Console.WriteLine("Error Binding: " + exc.Message + Environment.NewLine + exc.StackTrace);;
}


var transportScheduler = new IOQueue();
var applicationScheduler = PipeScheduler.ThreadPool;
var senderPool = new SenderPool();
var memoryPool = new PinnedBlockMemoryPool();

await AcceptConnections();
async Task AcceptConnections()
{
    while (true)
    {
        try
        {
            var socket = await listenSocket.AcceptAsync();
            var connection = new Connection(socket, senderPool,
                transportScheduler, applicationScheduler, memoryPool);
            _ = ProcessConnection(connection, port);
        }
        catch (System.Exception exc)
        {
            Console.WriteLine("Error Accepting Connection: " + exc.Message + Environment.NewLine + exc.StackTrace);;
        }
    }
}

static async Task ProcessConnection(Connection connection, string port)
{
    connection.Start();
    while (true)
    {
        ReadOnlySequence<byte> buffer;
        ReadResult result;
        try
        {
            result = await connection.Input.ReadAsync();
            buffer = result.Buffer;
            byte[] bufferArray = buffer.ToArray();
            var input = Encoding.UTF8.GetString(bufferArray, 0, bufferArray.Length);
            Console.WriteLine($"Socket Server received input: \"{input}\"");

            if (buffer.IsSingleSegment)
            {
                await connection.Output.WriteAsync(buffer.First);
            }
            else
            {
                foreach (var mem in buffer)
                {
                    await connection.Output.WriteAsync(mem);
                }
            }

            connection.Input.AdvanceTo(buffer.End);
            if (result.IsCompleted || result.IsCanceled)
            {
                break;
            }
        }
        catch (System.Exception exc)
        {
            Console.WriteLine("Error Reading: " + exc.Message + Environment.NewLine + exc.StackTrace); ;
        }

       
    }

    try
    {
        connection.Shutdown();
        await connection.DisposeAsync();
        Console.WriteLine($"Shut Down listener on port {port}.");
    }
    catch (System.Exception exc)
    {
        Console.WriteLine("Error Shutting Down: " + exc.Message + Environment.NewLine + exc.StackTrace); ;
    }
    
}