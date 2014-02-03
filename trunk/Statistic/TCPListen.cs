using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using System.Net;
using System.Net.Sockets;

using System.IO;

using StatisticCommon;

namespace Statistic
{
    public class TCPListen
    {
        Thread m_thread;
        List<TCPReciever> m_listThreadClient;
        int m_port;
        ManualResetEvent m_evClose;

        public TCPListen()
        {
            InitializeComponent();
            m_port = 6666;

            m_thread = new Thread(new ParameterizedThreadStart(Thread_Proc));
            m_thread.Name = "TCP - socket server listen";
            m_thread.IsBackground = true;

            m_listThreadClient = new List <TCPReciever> ();

            m_evClose = new ManualResetEvent (true); //.Set ();
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
            TcpListener tcpListener = new TcpListener(IPAddress.Any, m_port);
            tcpListener.Start();

            Console.WriteLine("The Server has started on port " + m_port);

            while (m_evClose.WaitOne() == true)
            {
                if (tcpListener.Pending() == true) {
                    m_listThreadClient.Add (new TCPReciever(tcpListener.AcceptTcpClient()));
                    m_listThreadClient [m_listThreadClient.Count - 1].Start ();
                }
                else {
                }               
            }

            for (int i = 0; i < m_listThreadClient.Count; i ++) {
                m_listThreadClient [i].Stop ();
            }
        }
    }
}