#define WAIT_CONNECTION

using System;
using System.IO;
using System.Net.Sockets;
using WiimoteLib;

namespace WiiboardClient
{
    class Program
    {
        private static Wiimote wiimote;

        private const float MIN_WEIGHT = 15F;
        private const string SEPARATOR = "|";
        private const int PORT = 12345;

        static void Main(string[] args)
        {
            try
            {
                ConnectWiimote();
                while (true)
                {
                    SendCenterOfGravity();
                    System.Threading.Thread.Sleep(10);
                }
            }
            catch (WiimoteException)
            {
                Console.WriteLine("WiimoteException: Cannot find a Wii Balance Board");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("InvalidOperationException: Are you using already the port {0}?", PORT);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }
        }

        static void ConnectWiimote()
        {
            wiimote = new Wiimote();
            wiimote.Connect();
            wiimote.SetLEDs(1);
            Console.WriteLine("Connected to Wii Balance Board!");
        }

        static void SendCenterOfGravity()
        {
            try
            {
                WiimoteState wiimoteState = wiimote.WiimoteState;
                BalanceBoardState bbs = wiimoteState.BalanceBoardState;

                string x = bbs.CenterOfGravity.X.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string y = bbs.CenterOfGravity.Y.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (bbs.WeightKg >= MIN_WEIGHT)
                {
                    TcpClient client = new TcpClient("127.0.0.1", PORT);
                    string message = x + SEPARATOR + y;
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                    client.Close();
                    stream.Close();
                }
            }
            catch (SocketException)
            {
#if WAIT_CONNECTION
                System.Threading.Thread.Sleep(5000);
#else
                Console.WriteLine("SocketException: Cannot connect to the port {0}", PORT);
#endif
            }
        }
    }
}
