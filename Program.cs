using cmd.httpserver;

class Program
{
    static Int32 port = 6969;

    public static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"[ERROR] {message}");
        Console.ResetColor();
    }

    async static Task Main()
    {
        try
        {
            var server = new Server();
            await server.Serve(port);
            Console.WriteLine($"Server started at port {port}");

            // This acts like the channel block '<-sigChan':w
            using var exitEvent = new ManualResetEventSlim(false);

            // Handle Ctrl+C (SIGINT)
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true; // Prevents immediate termination
                exitEvent.Set();
            };

            // Wait here indefinitely until the event is triggered
            exitEvent.Wait();

            Console.WriteLine("Server gracefully stopped");
        }
        catch (Exception ex)
        {
            LogError($"Error starting server: {ex.Message}");
        }
    }
}
