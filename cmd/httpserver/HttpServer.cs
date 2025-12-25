using System.Net.Sockets;
using cmd.tcplistener;
using request;
using response;

namespace cmd.httpserver;

public class Server
{
    public Dictionary<string, string> routes;

    public Server(Dictionary<string, string> Routes)
    {
        routes = Routes;
    }

    public string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath)?.ToLowerInvariant();

        var mappings = new Dictionary<string, string> {
        { ".html", "text/html" },
        { ".htm", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".json", "application/json" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" },
        { ".svg", "image/svg+xml" },
        { ".pdf", "application/pdf" },
        { ".txt", "text/plain" },
        { ".zip", "application/zip" },
        { ".mp3", "audio/mpeg" },
        { ".mp4", "video/mp4" },
        { ".wav", "audio/wav" },
        { ".epub", "application/epub+zip" }
    };

        if (extension != null && mappings.TryGetValue(extension, out var contentType))
        {
            return contentType;
        }

        return "application/octet-stream";
    }

    private async Task HandleRequest(Request req, NetworkStream stream)
    {
        var responseBytes = Handler(req);
        await stream.WriteAsync(responseBytes);
        await stream.FlushAsync();
    }

    private byte[] Handler(Request req)
    {
        if (req.requestLine.Method != "GET")
        {
            return (new Response("405")).GetBytes();
        }
        else
        {
            return GETHandler(req);
        }
    }

    private byte[] GETHandler(Request req)
    {
        foreach (var route in routes)
        {
            if (req.requestLine.Target == route.Key)
            {
                byte[] fileBytes = File.ReadAllBytes(route.Value);
                long totalSize = fileBytes.Length;
                string mimeType = GetMimeType(route.Value);

                Response res = new Response("200");
                res.SetHeader("Content-Type", mimeType);
                res.SetHeader("Accept-Ranges", "bytes");

                if (req.GetHeader("range") is string range)
                {
                    try
                    {
                        range = range.Replace("bytes=", "").Trim();
                        var rangeArr = range.Split("-");
                        int start = int.Parse(rangeArr[0]);
                        int end = string.IsNullOrEmpty(rangeArr[1])
                            ? (int)totalSize - 1
                            : int.Parse(rangeArr[1]);

                        res = new Response("206");
                        res.SetHeader("Content-Type", mimeType);
                        res.SetHeader("Accept-Ranges", "bytes");
                        res.SetHeader("Content-Range", $"bytes {start}-{end}/{totalSize}");

                        fileBytes = fileBytes[start..(end + 1)];
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error parsing the range header value");
                    }
                }

                res.SetBody(fileBytes);
                return res.GetBytes();
            }
        }
        return Error404();
    }

    private byte[] Error404()
    {
        Response res = new("404");
        byte[] body = "404 - Not Found"u8.ToArray();
        res.SetHeader("Content-Type", "text/plain");
        res.SetBody(body);
        return res.GetBytes();

    }

    public async Task Serve(Int32 port)
    {
        await TcpListenerService.StartListener(port, HandleRequest);
    }

    public void Stop()
    {

    }
}
