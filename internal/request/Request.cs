namespace request;

using System.Text;

public class RequestLine
{
    public string Method { get; set; }
    public string Target { get; set; }
    public string Version { get; set; }

    public RequestLine(string method, string target, string version)
    {
        Method = method;
        Target = target;
        Version = version;
    }
}

public enum ParserState
{
    Init,
    Headers,
    Done
}

public class Request
{
    public RequestLine requestLine { get; private set; } = new RequestLine("", "", "");
    public ParserState parserState { get; private set; } = ParserState.Init;

    private readonly List<byte> _buffer = new();

    public void ParseByte(byte currByte)
    {
        switch (parserState)
        {
            case ParserState.Init:
                _buffer.Add(currByte);
                if (currByte == 0x0A && _buffer.Count > 1 && _buffer[^2] == 0x0D)
                {
                    string requestLineString = Encoding.UTF8.GetString(_buffer.ToArray(), 0, _buffer.Count - 2);
                    ParseRequestLine(requestLineString);
                    _buffer.Clear();
                    parserState = ParserState.Headers;
                }

                if (_buffer.Count > 1024)
                {
                    throw new FormatException("Request line too long. Stop wasting my time.");
                }
                break;
            case ParserState.Headers:
                break;
        }


    }

    private void ParseRequestLine(string requestLineString)
    {
        var elements = requestLineString.Split(' ');

        if (elements.Length != 3) throw new FormatException($"Invalid Request Line {requestLineString}");

        var method = elements[0];
        var target = elements[1];
        var version = elements[2];

        if (!method.All(Char.IsUpper)) throw new FormatException($"Invalid Method: {method}");

        if (version != "HTTP/1.1") throw new FormatException($"Unsupported HTTP version: {version}");

        requestLine = new RequestLine(method, target, version);
    }
}
