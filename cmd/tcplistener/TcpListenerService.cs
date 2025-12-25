// TcpListenerService.cs
using System.Net;
using System.Net.Sockets;
using request;

namespace cmd.tcplistener;

static public class TcpListenerService
{
    public static async Task StartListener(Int32 port, Func<Request, NetworkStream, Task> callback)
    {
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();

            _ = Task.Run(async () =>
            {
                try
                {
                    using var stream = client.GetStream();
                    byte[] buffer = new byte[8192];

                    while (true)
                    {
                        var req = new Request(); // we want a new request object for each HTTP request
                        bool requestComplete = false;

                        while (!requestComplete)
                        {
                            int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0) return;

                            for (int i = 0; i < read; i++)
                            {
                                req.ParseByte(buffer[i]);
                                if (req.parserState == ParserState.Done)
                                {
                                    await callback(req, stream);
                                    requestComplete = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (IOException)
                {
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
