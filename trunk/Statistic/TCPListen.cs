using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

using System.Net;
using System.Net.Sockets;

using System.IO;

using StatisticCommon;

namespace Statistic
{
    public class TCPListen
    {
        Thread m_thread;
        int m_port;
        ManualResetEvent m_evClose;
        
        public TCPListen()
        {
            InitializeComponent();
            m_port = 6666;

            m_thread = new Thread(new ParameterizedThreadStart(Thread_Proc));
            m_thread.Name = "TCP - socket server listen";
            m_thread.IsBackground = true;

            m_evClose = new ManualResetEvent (true); //.Set (0;
        }

        private void InitializeComponent()
        {
            
        }

        public void Accept () {
            m_thread.Start ();
        }

        public void Close () {
            bool joined = false;

            if (m_thread.IsAlive)
            {
                m_evClose.Reset ();
                
                joined = m_thread.Join(6666);
                if (joined == false)
                    m_thread.Abort();
                else
                    ;
            }
            else
                ;
        }

        private void Thread_Proc(object data)
        {
            string msgOfClient = "Client connected";
            StreamWriter streamWriter;
            StreamReader streamReader;
            NetworkStream networkStream;
            TcpListener tcpListener = new TcpListener(IPAddress.Any, m_port);
            tcpListener.Start();

            Console.WriteLine("The Server has started on port " + m_port);
            Socket serverSocket = tcpListener.AcceptSocket();

            try
            {
                if (serverSocket.Connected)
                {
                    while (m_evClose.WaitOne () == true)
                    {
                        Console.WriteLine(msgOfClient);
                        networkStream = new NetworkStream(serverSocket);
                        streamWriter = new StreamWriter(networkStream);
                        streamReader = new StreamReader(networkStream);

                        msgOfClient = streamReader.ReadLine();
                    }
                }
                else
                    ;

                if (serverSocket.Connected)
                    serverSocket.Close();
                else
                    ;

                Console.Read();

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
    }
}