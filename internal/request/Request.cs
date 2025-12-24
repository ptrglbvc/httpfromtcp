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
    Body,
    Done
}

public class Request
{
    public ParserState parserState { get; private set; } = ParserState.Init;
    public RequestLine requestLine { get; private set; } = new RequestLine("", "", "");
    private Dictionary<string, string> _headers { get; } = new Dictionary<string, string>();
    public List<byte> body = new();

    private readonly List<byte> _buffer = new();
    private string _headerKey = "";
    private Int32 _contentLength = 0;

    public string? GetHeader(string key)
    {
        key = key.ToLower();
        if (_headers.ContainsKey(key))
        {
            return _headers[key];
        }
        else return null;
    }

    private void SetHeader(string key, string value)
    {
        key = key.ToLower();
        if (_headers.ContainsKey(key))
        {
            _headers[key] = _headers[key] + ", " + value;
        }
        else
        {
            _headers[key] = value;
        }
    }

    public void PrintHeaders()
    {
        foreach (KeyValuePair<string, string> entry in _headers)
        {
            Console.WriteLine($"{entry.Key}: {entry.Value}");
        }
    }

    private bool _IsValidToken(byte currByte)
    {
        // A-Z
        if (currByte >= 0x41 && currByte <= 0x5A) return true;
        // a-z
        if (currByte >= 0x61 && currByte <= 0x7A) return true;
        // 0-9
        if (currByte >= 0x30 && currByte <= 0x39) return true;
        // TODO: move to a static readonly variable, as its being created every time on the stack when the method is called
        // special chars
        return ((ReadOnlySpan<byte>)"!#$%&'*+-.^_`|~"u8).Contains(currByte);
    }

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
                ParseHeaderByte(currByte);
                break;
            case ParserState.Body:
                try
                {
                    var contentLength = Int32.Parse(_headers["content-length"]);
                    if (_buffer.Count < contentLength)
                    {
                        _buffer.Add(currByte);
                    }
                    if (_buffer.Count == contentLength)
                    {
                        body = _buffer;
                        parserState = ParserState.Done;
                    }
                    break;
                }
                catch (Exception)
                {
                    break;
                }

        }
    }

    public string BodyAsString()
    {
        return Encoding.UTF8.GetString(body.ToArray());
    }

    private void ParseHeaderByte(Byte currByte)
    {
        if (_headerKey == "")
        {
            // this means that there is whitespace in front of the header key, which is valid
            if (currByte == 0x20 && _buffer.Count == 0)
            {
                return;
            }
            // this means that there is whitespace in between the header key and :, which is invalid
            else if (currByte == 0x20 && _buffer.Count != 0)
            {
                throw new FormatException("Invalid header format.");
            }
            else if (currByte == 0x3A)
            {
                _headerKey = Encoding.UTF8.GetString(_buffer.ToArray());
                _buffer.Clear();
                return;
            }

            // allow \r into buffer to check for \r\n later
            else if (currByte == 0x0D)
            {
                _buffer.Add(currByte);
                return;
            }
            // this means that the line consists of \r\n, so we're done here chief
            else if (currByte == 0x0A && _buffer.Count == 1 && _buffer[0] == 0x0D)
            {
                _buffer.Clear();
                if (_headers.ContainsKey("content-length"))
                {
                    _contentLength = int.Parse(_headers["content-length"]);
                }
                else
                {
                    _contentLength = 0;
                }

                if (_contentLength == 0)
                {
                    parserState = ParserState.Done;
                }
                else
                {
                    parserState = ParserState.Body;
                }
                return;
            }
            else if (!_IsValidToken(currByte))
            {
                throw new FormatException("Invalid header field-name token");
            }
            else
            {
                _buffer.Add(currByte);
            }
        }

        else
        {
            // OWS after colon
            if (currByte == 0x20 && _buffer.Count == 0)
            {
                if (_buffer.Count == 0)
                {
                    return;
                }
                // we need whitespacees in some values, like ""Authorization: Bearer token"
                else
                {
                    _buffer.Add(currByte);
                }
            }
            // don
            else if (currByte == 0x0A)
            {
                if (_buffer[^1] == 0x0D)
                {
                    var value = Encoding.UTF8.GetString(_buffer[..^1].ToArray());
                    SetHeader(_headerKey, value.Trim());
                    _headerKey = "";
                    _buffer.Clear();
                }
                else throw new FormatException("Invalid header formatting");
            }
            else
            {
                _buffer.Add(currByte);
            }
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
