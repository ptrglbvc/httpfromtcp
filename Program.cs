using cmd.tcplistener;

class Program {
    async static Task Main() {
        await TcpListenerService.StartListener();
    }
}
