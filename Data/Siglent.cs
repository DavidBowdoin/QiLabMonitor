using System.Net;
using System.Net.Sockets;
using System.Text;

namespace QiLabMonitor
{
    public class Siglent
    {
        public bool active = false;
        private Socket socket;
        public string host = "192.168.1.18";
        public int port = 5025;

        public double setVolts = 3.44;
        public double setCurrent = 5;
        public double mesuredVolts = 0;
        public double mesuredCurrent = 0;
        public double mesuredPower = 0;
        public bool powerEnabled = false;

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
                double.TryParse(SocketSendReceive(socket, "MEASure:VOLTage?"), out mesuredVolts);
                double.TryParse(SocketSendReceive(socket, "MEASure:CURRent?"), out mesuredCurrent);
                double.TryParse(SocketSendReceive(socket, "MEASure:POWEr?"), out mesuredPower);
                double.TryParse(SocketSendReceive(socket, "CH1:VOLTage?"), out setVolts);
                double.TryParse(SocketSendReceive(socket, "CH1:CURRent?"), out setCurrent);
                var status = Convert.ToInt32(SocketSendReceive(socket, "SYSTem:STATus?").Trim(), 16);
                powerEnabled = (status & 16) == 16;
            }
        }

        public void ToggleOutput()
        {
            if (active) SocketSend(socket, "OUTPut CH1," + (powerEnabled ? "OFF" : "ON"));
        }

        public void SetOutput(bool _powerEnabled)
        {
            if (active) SocketSend(socket, "OUTPut CH1," + (_powerEnabled ? "ON" : "OFF"));
        }

        public void SetVoltage(double volts)
        {
            int temp = (int)(volts * 1000.0);
            setVolts = temp / 1000.0;
            if (active) SocketSend(socket, "CH1:VOLTage " + setVolts);
        }

        void TestProgram()
        {
            //Console.Write(SocketSendReceive(socket, "*IDN?"));
            //Console.Write(SocketSendReceive(socket, "INSTrument?"));
            //SocketSend(socket, "OUTPut CH1,ON");
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1000));
            //Thread.Sleep(5000);
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1100));
            //Thread.Sleep(5000);
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1200));
            //Thread.Sleep(5000);
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1300));
            //Thread.Sleep(5000);
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1400));
            //Thread.Sleep(5000);
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1500));
            //Thread.Sleep(5000);
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1600));
            //Thread.Sleep(5000);
            //SocketSend(socket, "CH1:VOLTage " + calculateVoltage(1700));
            //SocketSend(socket, "OUTPut CH1,OFF");
            //Console.Write(SocketSendReceive(socket, "MEASure:VOLTage?"));
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
