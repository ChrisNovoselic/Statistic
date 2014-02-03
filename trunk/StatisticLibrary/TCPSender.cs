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

        TcpClient m_tcpClient;
        NetworkStream m_networkStream;
        StreamReader m_streamReader;
        StreamWriter m_streamWriter;

        public TCPSender()
        {
            InitializeComponent();

            m_port = 6666;
            m_tcpClient = null;
        }

        private void InitializeComponent()
        {

        }

        public void Init () {
            string response;

            try
            {
                m_tcpClient = new TcpClient("localhost", m_port);

                m_networkStream = m_tcpClient.GetStream();
                //m_streamReader = new StreamReader(m_networkStream);
                m_streamWriter = new StreamWriter(m_networkStream);

                m_streamWriter.WriteLine("INIT");
                m_streamWriter.Flush();

                m_networkStream = m_tcpClient.GetStream();
                m_streamReader = new StreamReader(m_networkStream);
                //m_streamWriter = new StreamWriter(m_networkStream);
                
                response = m_streamReader.ReadLine();
                if (response == "Ok")
                    ;
                else
                    ;
            }

            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Close () {
            if (!(m_tcpClient == null))
            {
                m_tcpClient.Close ();
            }
            else
                ;
        }
    }
}