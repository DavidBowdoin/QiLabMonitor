using System.Net;
using System.Net.Sockets;
using System.Text;

namespace QiLabMonitor
{
    public class Siglent
    {
        public bool active = false;
        private Socket socket;
        public string host = "192.168.1.24";
        public int port = 5025;

        public double setCurrent = 5;
        public double mesuredVolts = 0;
        public double mesuredCurrent = 0;
        public double mesuredPower = 0;

        private double _setVolts = 3.44;
        public double setVolts
        {
            get => _setVolts;
            set
            {
                int temp = (int)(value * 1000.0);
                if (temp / 1000.0 != _setVolts)
                {
                    _setVolts = temp / 1000.0;
                    if (active) SocketSend(socket, "CH1:VOLTage " + setVolts);
                }
            }
        }

        private bool _powerEnabled = false;
        public bool powerEnabled
        {
            get => _powerEnabled;
            set
            {
                if (value != _powerEnabled)
                {
                    if (active) SocketSend(socket, "OUTPut CH1," + (value ? "ON" : "OFF"));
                }
            }
        }

        public void Start()
        {
            socket = ConnectSocket(host, port);
            if (socket.Connected)
            {
                active = true;
                ReadValues();
            }
            else
            {
                Console.WriteLine("Siglent Connection Failed");
            }
        }

        public void ReadValues()
        {
            if (active)
            {
                string _systemStatus;
                double.TryParse(SocketSendReceive(socket, "MEASure:VOLTage?"), out mesuredVolts);
                double.TryParse(SocketSendReceive(socket, "MEASure:CURRent?"), out mesuredCurrent);
                double.TryParse(SocketSendReceive(socket, "MEASure:POWEr?"), out mesuredPower);
                double.TryParse(SocketSendReceive(socket, "CH1:VOLTage?"), out _setVolts);
                double.TryParse(SocketSendReceive(socket, "CH1:CURRent?"), out setCurrent);
                try
                {
                    _systemStatus = SocketSendReceive(socket, "SYSTem:STATus?").Trim();
                    var status = Convert.ToInt32(_systemStatus, 16);
                    _powerEnabled = (status & 16) == 16;
                }
                catch (Exception e)
                {
                }
            }
        }

        Socket ConnectSocket(string ip, int port)
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
            Socket socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipe);
            return socket;
        }

        void SocketSend(Socket s, string message)
        {
            byte[] bytesSent = Encoding.ASCII.GetBytes(message + "\n");
            s.Send(bytesSent, bytesSent.Length, 0);
            Thread.Sleep(100);
        }

        string SocketSendReceive(Socket s, string message)
        {
            byte[] bytesSent = Encoding.ASCII.GetBytes(message + "\n");
            byte[] bytesReceived = new byte[4096];

            s.Send(bytesSent, bytesSent.Length, 0);
            int bytes = s.Receive(bytesReceived, bytesReceived.Length, 0);
            return Encoding.ASCII.GetString(bytesReceived, 0, bytes);
        }


    }
}
