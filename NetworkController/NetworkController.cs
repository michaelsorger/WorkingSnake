using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkController
{
    public delegate void EventProcessor(SocketState s);

    public class SocketState
    {
        public Socket theSocket;
        public int ID;
        public EventProcessor callMe;
        // This is the buffer where we will receive message data from the client
        public byte[] messageBuffer = new byte[1024];

        // This is a larger (growable) buffer, in case a single receive does not contain the full message.
        public StringBuilder sb = new StringBuilder();

        public SocketState(Socket s, int id)
        {
            theSocket = s;
            ID = id;
        }

        public SocketState()
        {
            
        }
    }

    /// <summary>
    /// generic networking class to help send and recieve data from the client to the server
    /// </summary>
    public static class Network
    {
        //should functions be public? private? protected? designate server vs client code?
        //two handshakes? one between client and network, then network and server?
        //network protocol, is that implemented throughout our functions as described in ps7?

        public const int DEFAULT_PORT = 11000;
        static List<SocketState> clients;
        static TcpListener listener;
        public static SocketState stateToPass;
        static Network()
        {

            listener = new TcpListener(IPAddress.Any, DEFAULT_PORT);
            clients = new List<SocketState>();
        }
        /// <summary>
        /// attempt to connect to the server via a provided hostname. 
        /// save the callback function (in a socket state object) for use when data arrives.
        /// It will need to open a socket and then use the BeginConnect method.
        /// 
        /// Note this method takes the "state" object and "regurgitates" it back to you when a connection is made, 
        /// thus allowing "communication" between this function and the ConnectedToServer function.
        /// </summary>
        /// <param name="callbackFunction"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>

        /// <summary>
        /// Start attempting to connect to the server
        /// </summary>
        /// <param name="host_name"> server to connect to </param>
        /// <returns></returns>
        public static Socket ConnectToServer(EventProcessor callbackFunction, string hostName)
        {
            System.Diagnostics.Debug.WriteLine("connecting  to " + hostName);

            // Connect to a remote device.
            try
            {

                // Establish the remote endpoint for the socket.
                IPHostEntry ipHostInfo;
                IPAddress ipAddress = IPAddress.None;

                // Determine if the server address is a URL or an IP
                try
                {
                    ipHostInfo = Dns.GetHostEntry(hostName);
                    bool foundIPV4 = false;
                    foreach (IPAddress addr in ipHostInfo.AddressList)
                        if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                        {
                            foundIPV4 = true;
                            ipAddress = addr;
                            break;
                        }
                    // Didn't find any IPV4 addresses
                    if (!foundIPV4)
                    {
                        System.Diagnostics.Debug.WriteLine("Invalid addres: " + hostName);
                        return null;
                    }
                }
                catch (Exception e1)
                {
                    // see if host name is actually an ipaddress, i.e., 155.99.123.456
                    System.Diagnostics.Debug.WriteLine("using IP");
                    ipAddress = IPAddress.Parse(hostName);
                }

                // Create a TCP/IP socket.
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

                stateToPass = new SocketState();
                stateToPass.theSocket = socket;
                stateToPass.callMe = callbackFunction;

                socket.BeginConnect(ipAddress, DEFAULT_PORT, ConnectedToServer, stateToPass);
                return socket;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return null;
            }
        }

        /// <summary>
        /// This function is referenced by the BeginConnect method above and is "called" by the OS when the socket
        /// connects to the server. The "state_in_an_ar_object" object contains a field "AsyncState" which contains 
        /// the "state" object saved away in the above function.
        //  Once a connection is established the "saved away" callbackFunction needs to called.
        /// This function is saved in the socket state, and was originally passed in to ConnectToServer.
        /// </summary>
        /// <param name="state_in_an_ar_object"></param>
        private static void ConnectedToServer(IAsyncResult ar)
        {
            stateToPass = (SocketState)ar.AsyncState;

            try
            {
                // Complete the connection.
                stateToPass.theSocket.EndConnect(ar);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Unable to connect to server. Error occured: " + e);
                return;
            }
            stateToPass.callMe(stateToPass);
            // TODO: If we had a "EventProcessor" delagate stored in the state, we could call that,
            //       instead of hard-coding a method to call.
            stateToPass.theSocket.BeginReceive(stateToPass.messageBuffer, 0, stateToPass.messageBuffer.Length,SocketFlags.None,RecieveCallback, stateToPass);

        }

        ///The ReceiveCallback method is called by the OS when new data arrives.
        ///This method should check to see how much data has arrived. 
        ///If 0, the connection has been closed (presumably by the server). 
        ///On greater than zero data, this method should call the callback function provided above.
        public static void RecieveCallback(IAsyncResult state_in_an_ar_object)
        {
            stateToPass = (SocketState)state_in_an_ar_object.AsyncState;



            ///ERROR THROWS HERE. WHY??????
            ///

//change

            int bytesRead = stateToPass.theSocket.EndReceive(state_in_an_ar_object);
            if (bytesRead > 0)
            {
                string theMessage = Encoding.UTF8.GetString(stateToPass.messageBuffer, 0, bytesRead);
                stateToPass.sb.Append(theMessage);
                stateToPass.callMe(stateToPass);
            }
        }
        /// <summary>
        /// This is a small helper function that the client View code will call whenever it wants more data... out of what?
        /// Note: the client will probably want more data every time it gets data, 
        /// and has finished processing it in its callbackFunction.
        /// </summary>
        /// <param name=""></param>
        public static void GetData(SocketState state)
        {
            stateToPass.theSocket.BeginReceive(stateToPass.messageBuffer, 0, stateToPass.messageBuffer.Length, SocketFlags.None, RecieveCallback, stateToPass);
        }

        /// <summary>
        /// This function (along with its helper 'SendCallback') will allow a program to send data over a socket. 
        /// This function needs to convert the data into bytes and then send them using socket.BeginSend.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="data"></param>
        public static void Send(SocketState ss, String data)
        {

            byte[] messageBytes = Encoding.UTF8.GetBytes(data + "\n");
            
            ss.theSocket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, ss);
        }

        /// <summary>
        /// This function "assists" the Send function. If all the data has been sent,
        /// then life is good and nothing needs to be done 
        /// (note: you may, when first prototyping your program, put a WriteLine in here to see when data goes out).
        /// </summary>
        private static void SendCallback(IAsyncResult ar)
        {
            SocketState ss = (SocketState)ar.AsyncState;
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            ss.theSocket.EndSend(ar);
        }
    }    
}
