using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace StatisticCommon
{
    public class TcpClientAsync
    {
        private IPAddress[] addresses;
        private int port;
        private WaitHandle addressesSet;
        private TcpClient tcpClient;
        private int failedConnectionCount;

        private ManualResetEvent m_evDisconnect
                                , m_evConnect;

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
            this.addresses = addresses;
        }

        /// <summary>
        /// Construct a new client where the address or host name of
        /// the server is known.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or address of the server</param>
        /// <param name="port">The port of the server</param>
        public TcpClientAsync(string hostNameOrAddress, int port) : this (port)
        {
            addressesSet = new AutoResetEvent(false);
            Dns.BeginGetHostAddresses(hostNameOrAddress, GetHostAddressesCallback, null);
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

            this.port = port;
            this.tcpClient = new TcpClient();
            this.Encoding = Encoding.Default;

            m_evConnect = new ManualResetEvent (true);
            m_evDisconnect = new ManualResetEvent(false);
        }

        /// <summary>
        /// The endoding used to encode/decode string when sending and receiving.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Attempts to connect to one of the specified IP Addresses
        /// </summary>
        public void Connect()
        {
            m_evConnect.Reset ();
            
            if (!(addressesSet == null))
                //Wait for the addresses value to be set
                addressesSet.WaitOne();
            else
                ;

            //Set the failed connection count to 0
            Interlocked.Exchange(ref failedConnectionCount, 0);
            //Start the async connect operation
            tcpClient.BeginConnect(addresses, port, ConnectCallback, null);
        }

        /// <summary>
        /// Writes a string to the network using the defualt encoding.
        /// </summary>
        /// <param name="data">The string to write</param>
        /// <returns>A WaitHandle that can be used to detect
        /// when the write operation has completed.</returns>
        public void Write(string data)
        {
            byte[] bytes = Encoding.GetBytes(data);
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
            if (m_evConnect.WaitOne (66) == true)
            {
                NetworkStream networkStream = null;
                try { networkStream = tcpClient.GetStream(); }
                catch (Exception e)
                {
                    Logging.Logg().LogExceptionToFile(e, "TCPClientAsync::Write(byte[] ) - ...");
                }
                //Start async write operation
                networkStream.BeginWrite(bytes, 0, bytes.Length, WriteCallback, null);
            }
            else
                //Нет соединения
                m_evDisconnect.Set ();
        }

        /// <summary>
        /// Callback for Write operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void WriteCallback(IAsyncResult result)
        {
            NetworkStream networkStream = tcpClient.GetStream();
            networkStream.EndWrite(result);

            if (!(m_evDisconnect == null))
                m_evDisconnect.Set ();
            else
                ;

        }

        /// <summary>
        /// Callback for Connect operation
        /// </summary>
        /// <param name="result">The AsyncResult object</param>
        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                tcpClient.EndConnect(result);
            }
            catch
            {
                //Increment the failed connection count in a thread safe way
                Interlocked.Increment(ref failedConnectionCount);

                if (!(failedConnectionCount < addresses.Length))
                {
                    //We have failed to connect to all the IP Addresses
                    //connection has failed overall.
                }
                else
                    ;

                return;
            }

            //We are connected successfully.
            NetworkStream networkStream = tcpClient.GetStream();
            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

            //Now we are connected start asyn read operation.
            networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);

            m_evConnect.Set ();
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
                networkStream = tcpClient.GetStream(); 
                read = networkStream.EndRead(result);
            }
            catch (Exception excpt)
            {
                //An error has occured when reading
                Console.WriteLine("TCPClientAsync::ReadCallback () - An error has occured when reading...type of exception: " + excpt.GetType().FullName);
            }

            if (read == 0)
            {
                //The connection has been closed.
                Disconnect ();

                return;
            }
            else
                ;

            if (! (networkStream == null))
            {
                byte[] buffer = result.AsyncState as byte[];
                string data = this.Encoding.GetString(buffer, 0, read);

                //Do something with the data object here.
                Console.WriteLine(((IPEndPoint)tcpClient.Client.LocalEndPoint).Address + ": " + data);

                //Then start reading from the network again.
                networkStream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
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
            addresses = Dns.EndGetHostAddresses(result);

            //Signal the addresses are now set
            ((AutoResetEvent)addressesSet).Set();
        }

        public void Disconnect()
        {
            Write (@"DISCONNECT");
            m_evDisconnect.WaitOne ();
            
            if (!(tcpClient == null))
            {
                try { 
                    tcpClient.GetStream ().Close ();
                    tcpClient.Close();
                }
                catch (Exception e)
                {
                    Logging.Logg ().LogExceptionToFile (e, "TCPClientAsync::Disconnect () - ...");
                }
            }
            else
                ;

            m_evConnect.Reset();
        }
    }
}