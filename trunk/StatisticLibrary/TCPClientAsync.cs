using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace StatisticCommon
{
    public class TcpClientAsync
    {
        //private enum STATE {READ, WRITE, COUNT_STATE};

        private IPAddress[] m_addresses;
        private int m_port;
        private WaitHandle m_addressesSet,
                m_addressesGet;
        private TcpClient m_tcpClient;
        private int m_failedConnectionCount;

        private string m_ValueToCreate;

        //private ManualResetEvent[] m_arEvState;
        private AutoResetEvent m_evWrite;

        public DelegateStringFunc delegateErrorConnect;
        public delegate void DelegateTcpAsyncFunc(TcpClient res, string data);
        public DelegateTcpAsyncFunc delegateConnect, delegateRead;

        /// <summary>
        /// Construct a new client from a known IP Address
        /// </summary>
        /// <param name="address">The IP Address of the server</param>
        /// <param name="port">The port of the server</param>
        public TcpClientAsync(IPAddress address, int port) : this (new[] { address }, port) { }

        /// <summary>
        /// Construct a new client where multiple IP Addresses for
        /// the same client are known.
        /// </summary>
        /// <param name="addresses">The array of known IP Addresses</param>
        /// <param name="port">The port of the server</param>
        public TcpClientAsync(IPAddress[] addresses, int port) : this (port)
        {
            m_ValueToCreate = addresses.ToString ();
            this.m_addresses = addresses;
        }

        /// <summary>
        /// Construct a new client where the address or host name of
        /// the server is known.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address of the server</param>
        /// <param name="port">The port of the server</param>
        public TcpClientAsync(string hostNameOrAddress, int port) : this (port)
        {
            m_ValueToCreate = hostNameOrAddress;
            m_addressesSet = new AutoResetEvent(false);
            m_addressesGet = new AutoResetEvent(false);
            Dns.BeginGetHostAddresses(hostNameOrAddress.Split (';')[0], GetHostAddressesCallback, null);
        }

        /// <summary>
        /// Private constuctor called by other constuctors
        /// for common operations.
        /// </summary>
        /// <param name="port"></param>
        private TcpClientAsync(int port)
        {
            if (port < 0)
                throw new ArgumentException();
            else
                ;

            this.m_port = port;
            this.m_tcpClient = new TcpClient();
            this.m_Encoding = Encoding.Default;

            m_evWrite = new AutoResetEvent (false);
        }

        public TcpClientAsync()
        {
            this.m_tcpClient = new TcpClient();
            this.m_Encoding = Encoding.Default;

            m_evWrite = new AutoResetEvent(false);
        }

        /// <summary>
        /// The endoding used to encode/decode string when sending and receiving.
        /// </summary>
        public Encoding m_Encoding { get; set; }

        public void Connect(IPAddress addresses, int port)
        {
            m_ValueToCreate = addresses.ToString ();
            this.m_addresses = new [] {addresses};
            this.m_port = port;

            ((AutoResetEvent)m_evWrite).Reset();
        }
        
        public void Connect(IPAddress[] addresses, int port)
        {
            m_ValueToCreate = addresses.ToString();
            this.m_addresses = addresses;
            this.m_port = port;

            ((AutoResetEvent)m_evWrite).Reset();
        }

        public void Connect(string hostNameOrAddress, int port)
        {
            m_ValueToCreate = hostNameOrAddress;
            if (m_addressesGet == null)
                m_addressesGet = new AutoResetEvent (false);
            else
                ((AutoResetEvent)m_addressesGet).Reset();

            if (m_addressesSet == null)
                m_addressesSet = new AutoResetEvent (false);
            else
                ((AutoResetEvent)m_addressesSet).Reset();

            this.m_port = port;

            ((AutoResetEvent)m_evWrite).Reset();
            
            Dns.BeginGetHostAddresses(hostNameOrAddress.Split (';')[0], GetHostAddressesCallback, null);

            Connect ();
        }

        /// <summary>
        /// Attempts to connect to one of the specified IP Addresses
        /// </summary>
        public void Connect()
        {
            int indxAddresses = -1;
            if ((!(m_addressesSet == null)) && (!(m_addressesGet == null)))
                //Wait for the addresses value to be set
                indxAddresses = WaitHandle.WaitAny(new WaitHandle[] { m_addressesGet, m_addressesSet });
            else
                ;

            switch (indxAddresses)
            {
                case 0:
                    ErrorConnect ();
                    break;
                case 1:
                    //Set the failed connection count to 0
                    Interlocked.Exchange(ref m_failedConnectionCount, 0);

                    if (m_tcpClient.Connected == false)
                        m_tcpClient = new TcpClient ();
                    else
                        ;

                    //Start the async connect operation
                    m_tcpClient.BeginConnect(m_addresses, m_port, ConnectCallback, null);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Writes a string to the network using the defualt encoding.
        /// </summary>
        /// <param name="data">The string to write</param>
        /// <returns>A WaitHandle that can be used to detect
        /// when the write operation has completed.</returns>
        public void Write(string data)
        {
            byte[] bytes = m_Encoding.GetBytes(data);
            Write(bytes);
        }

        /// <summary>
        /// Writes an array of bytes to the network.
        /// </summary>
        /// <param name="bytes">The array to write</param>
        /// <returns>A WaitHandle that can be used to detect
        /// when the write operation has completed.</returns>
        public void Write(byte[] bytes)
        {
            if (m_evWrite.WaitOne (666) == true)
            {
                NetworkStream networkStream = null;
                try { networkStream = m_tcpClient.GetStream(); }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "TCPClientAsync::Write(byte[] ) - ...");
                }
                //Start async write operation
                networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, null);
            }
            else
                ;
        }

        /// <summary>
        /// Callback for Write operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void WriteCallback(IAsyncResult result)
        {
            NetworkStream networkStream = m_tcpClient.GetStream();
            networkStream.EndWrite(result);

            m_evWrite.Set();

        }

        /// <summary>
        /// Callback for Connect operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                m_tcpClient.EndConnect(result);
            }
            catch (Exception e)
            {
                //Increment the failed connection count in a thread safe way
                Interlocked.Increment(ref m_failedConnectionCount);

                if (!(m_failedConnectionCount < m_addresses.Length))
                {
                    //We have failed to connect to all the IP Addresses
                    //connection has failed overall.

                    ErrorConnect ();
                }
                else
                    ;

                Console.WriteLine("TCPClientAsync::ConnectCallback () - An error has occured when call this methode...type of exception: " + e.GetType().FullName);

                return;
            }

            //We are connected successfully.
            NetworkStream networkStream = m_tcpClient.GetStream();
            byte[] buffer = new byte[m_tcpClient.ReceiveBufferSize];

            //Now we are connected start asyn read operation.
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);

            m_evWrite.Set ();

            delegateConnect(m_tcpClient, null);
        }

        public bool Equals (TcpClient obj)
        {
            return m_tcpClient.Equals (obj);
        }

        public bool Equals(string ValueToCreate)
        {
            return m_ValueToCreate.Equals(ValueToCreate);
        }

        /// <summary>
        /// Callback for Read operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ReadCallback(IAsyncResult result)
        {
            int read = 0;
            NetworkStream networkStream = null;

            try
            {
                networkStream = m_tcpClient.GetStream(); 
                read = networkStream.EndRead(result);
            }
            catch (Exception excpt)
            {
                //An error has occured when reading
                Console.WriteLine("TCPClientAsync::ReadCallback () - An error has occured when call this methode...type of exception: " + excpt.GetType().FullName);
            }

            if (read == 0)
            //if ((read == 0) && (networkStream == null))
            {
                //The connection has been closed.
                m_evWrite.Reset();

                return;
            }
            else
                ;

            if (! (networkStream == null))
            {
                byte[] buffer = result.AsyncState as byte[];
                string data = this.m_Encoding.GetString(buffer, 0, read);

                //Do something with the data object here.
                Console.WriteLine(((IPEndPoint)m_tcpClient.Client.LocalEndPoint).Address + ": " + data);

                delegateRead(m_tcpClient, data);

                if ((!(m_tcpClient == null)) && (m_tcpClient.Connected == true))
                    //Then start reading from the network again.
                    networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                else
                    ;
            }
            else
                ;
        }

        /// <summary>
        /// Callback for Get Host Addresses operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void GetHostAddressesCallback(IAsyncResult result)
        {
            try {
                m_addresses = Dns.EndGetHostAddresses(result);
            }
            catch (Exception e)
            {
                Console.WriteLine("TCPClientAsync::GetHostAddressesCallback () - Dns.EndGetHostAddresses()");

                ((AutoResetEvent)m_addressesGet).Set();
                
                return;
            }

            //Signal the m_addresses are now set
            ((AutoResetEvent)m_addressesSet).Set();
        }

        public void Disconnect()
        {
            m_evWrite.WaitOne (666);

            if (!(m_tcpClient == null))
            {
                if (m_tcpClient.Client.Connected == true)
                {                    
                    try { m_tcpClient.Client.Shutdown (SocketShutdown.Both); }
                    catch (Exception e)
                    {
                        Console.WriteLine("TCPClientAsync::Disconnect () - tcpClient.Client.Shutdown (SocketShutdown.Both) - An error has occured when call this methode...type exception: " + e.GetType().FullName);
                    }

                    try { m_tcpClient.Close(); }
                    catch (Exception e)
                    {
                        //Logging.Logg().LogExceptionToFile(e, "TCPClientAsync::Disconnect () - tcpClient.Close()");
                        Console.WriteLine("TCPClientAsync::Disconnect () - tcpClient.Close () - An error has occured when call this methode...type exception: " + e.GetType().FullName);
                    }

                    //m_tcpClient = null;
                }
                else
                    ;
            }
            else
                ;
        }

        private void ErrorConnect ()
        {
            delegateErrorConnect(m_ValueToCreate);
        }
    }
}