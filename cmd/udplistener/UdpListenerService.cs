using System.Net.Sockets;
using System.Text;

namespace cmd.udpsender;

class UdpSender {
    public static void SEND() {
        UdpClient udpClient = new UdpClient(6968);
        try {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (input == null) { input = "boobs"; }

            udpClient.Connect("localhost", 6969);
            Byte[] sendBytes = Encoding.ASCII.GetBytes(input);
            udpClient.Send(sendBytes, sendBytes.Length);

            udpClient.Close();
        } catch (Exception e) {
            Console.WriteLine(e.ToString());
        }

    }

}
