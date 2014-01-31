using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

using System.Net;
using System.Net.Sockets;

using System.IO;

namespace StatisticCommon
{
    public class TCPSender
    {
        int m_port;

        public TCPSender()
        {
            InitializeComponent();

            m_port = 6666;
            TcpClient tcpClient;
            NetworkStream networkStream;
            StreamReader streamReader;
            StreamWriter streamWriter;

            try
            {
                tcpClient = new TcpClient("localhost", m_port);
                networkStream = tcpClient.GetStream();
                streamReader = new StreamReader(networkStream);
                streamWriter = new StreamWriter(networkStream);
                streamWriter.WriteLine("Message from the Client...");
                streamWriter.Flush();
            }

            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void InitializeComponent()
        {

        }
    }
}