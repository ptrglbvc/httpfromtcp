using System.Net;
using System.Net.Sockets;
using request;

namespace cmd.tcplistener;

static public class TcpListenerService
{
    public static async Task StartListener()
    {
        int PORT = 6969;
        var listener = new TcpListener(IPAddress.Any, PORT);
        listener.Start();

        Console.WriteLine($"Listening on port {PORT}...");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    using var stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    var req = new Request();
                    while (true)
                    {
                        int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0) break;

                        for (int i = 0; i < read; i++)
                        {
                            req.ParseByte(buffer[i]);
                            if (req.parserState == ParserState.Headers && req.requestLine != null)
                            {
                                Console.WriteLine($"[SUCCESS] Parsed Method: {req.requestLine.Method}");
                                Console.WriteLine($"[SUCCESS] Parsed Target: {req.requestLine.Target}");
                                Console.WriteLine($"[SUCCESS] Parsed Version: {req.requestLine.Version}");
                                // You might want to break here for now until Headers logic is written
                            }
                            if (req.parserState == ParserState.Done) break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    client.Dispose();
                }
            });
        }
    }
}
