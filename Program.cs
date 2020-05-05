using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SampleSocket
{
    class Program
    {
        static string ip = "127.0.0.1";
        static Int32 port = 11000;
        static void Main(string[] args)
        {

            Task.Run(() => StartListening());

            ///Task.Run(() => StartClient());

            Console.ReadLine();
        }
        static int counter = 0;
        public static void StartListening()
        {
            string data = null;
            // Data buffer for incoming data.  
            byte[] bytes = new Byte[1024];
            byte[] msg = new Byte[1024];
            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            //IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(localEndPoint.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    data = null;
                    Console.WriteLine("connected");
                    // An incoming connection needs to be processed.  
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                        if (!string.IsNullOrEmpty(data))
                        {

                            // Show the data on the console.  
                            Console.WriteLine("Text received : {0}", data);

                            switch (data.ToLower())
                            {
                                case "increment":
                                    Increment();
                                    break;
                                case "evaluate":
                                    int maxvalue = Evaluate();
                                    data += " :" + maxvalue.ToString();
                                    break;
                            }

                            // Echo the data back to the client.  
                            msg = Encoding.ASCII.GetBytes(data);

                            handler.Send(msg);
                        }
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        private static void Increment()
        {
            int maxvalue = 0;
            string strqry = "SELECT MAX(Value) FROM Count";
            DataTable dtMax = DBConnectionManager.Instance.GetData(strqry);
            if (dtMax != null && dtMax.Rows.Count > 0&& dtMax.Rows[0][0]!=DBNull.Value)
            {
                maxvalue = Convert.ToInt32(dtMax.Rows[0][0])+1;
            }

            strqry = string.Format("INSERT INTO Count(Value) VALUES ({0})", maxvalue);
            DBConnectionManager.Instance.ExecuteQuery(strqry);
            Console.WriteLine("Max:" + maxvalue.ToString());
        }

        private static int Evaluate()
        {
            int maxvalue = 0;
            string strqry = "SELECT MAX(Value) FROM Count";
            DataTable dtMax = DBConnectionManager.Instance.GetData(strqry);
            if (dtMax != null && dtMax.Rows.Count > 0)
            {
                maxvalue = Convert.ToInt32(dtMax.Rows[0][0]);
            }
            Console.WriteLine("Eval Max:" + maxvalue.ToString());
            return maxvalue;
        }

        public static void StartClient()
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    //-------------------------------------------------------------

                    bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                     bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
