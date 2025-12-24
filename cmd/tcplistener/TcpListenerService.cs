using System.Net;
using System.Net.Sockets;
using request;

namespace cmd.tcplistener;

static public class TcpListenerService
{
    public static async Task StartListener(Int32 port, Action<Request, NetworkStream> callback)
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
                    byte[] buffer = new byte[1024];
                    var req = new Request();
                    while (true)
                    {
                        int read = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0) break;

                        for (int i = 0; i < read; i++)
                        {
                            req.ParseByte(buffer[i]);
                            if (req.parserState == ParserState.Done)
                            {
                                callback(req, stream);
                                break;
                            }

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
