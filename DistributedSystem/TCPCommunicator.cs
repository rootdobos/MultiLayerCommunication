using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Communication;
using System.IO;


namespace DistributedSystem
{
    public class TCPCommunicator
    {
        public void AddSystemExecutor(string system, PipelineExecutor executor)
        {
            _Executors.Add(system, executor);
        }
        //public TCPCommunicator(int index,string hostIP, int hubPort, int myPort, Queue<Message> eventQueue, object locker)
        public TCPCommunicator(int index,string hubIP, int hubPort,string myIp, int myPort)
        {
            _Executors = new Dictionary<string, PipelineExecutor>();
            _Index = index;
            //_EventQueue = eventQueue;
            _MyIP = System.Net.IPAddress.Parse(myIp);
            _MyEndPoint = new IPEndPoint(_MyIP, myPort);
            _ListeningSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            List<object> listeningThreadParameters = new List<object>();
            listeningThreadParameters.Add(_ListeningSocket);
            listeningThreadParameters.Add(_MyEndPoint);
            //listeningThreadParameters.Add(_EventQueue);
            //listeningThreadParameters.Add(locker);

            Thread t = new Thread(new ParameterizedThreadStart(TCPDeliver));
            t.Start(listeningThreadParameters);
            _HubIP = System.Net.IPAddress.Parse(hubIP);
            _HubEndPoint = new IPEndPoint(_HubIP, hubPort);

            // w = new StreamWriter(@"D:\egyetem\2 felev\algmoco\asigment\log.txt", true);
            // w2 = new StreamWriter(@"D:\egyetem\2 felev\algmoco\asigment\log2.txt", true);

        }
       //StreamWriter w;
       // StreamWriter w2;
        //public void SetExecutor(PipelineExecutor executor)
        //{
        //    _Executor = executor;
        //}
        public  void TCPDeliver(object parameters)
        {
            List<object> parameterList = (List<object>)parameters;
            Socket listeningSocket = (Socket)parameterList[0];
            IPEndPoint ip = (IPEndPoint)parameterList[1];
            //Queue<Message> eventQueue = (Queue<Message>)parameterList[2];
            //object locker = parameterList[3];
            listeningSocket.Bind(ip);
            listeningSocket.Listen(100);
            Console.WriteLine("{0} Listening to a new messages ... ", _Index);
            while (true)
            {

                //Console.WriteLine("{0} Listening to a new message ... ", _Index);
                Socket clientSocket = listeningSocket.Accept();
                int offset = 0;
                byte[] buffer = new byte[32768];
                int size = 32768;
                int readBytes;
                do
                {
                    readBytes = clientSocket.Receive(buffer, offset, buffer.Length - offset,
                                               SocketFlags.None);

                    offset += readBytes;
                    if(offset>4)
                        size = GetSizeFromByteArray(buffer);
                } while ((readBytes > 0 && offset < buffer.Length) && offset<size);

                //int numByte = clientSocket.Receive(buffer,0,buffer.Length-offset, SocketFlags.None);
                //readBytes = socket.Receive(buffer, offset, buffer.Length - offset,
                //                           SocketFlags.None);
                
                byte[] data = GetDataFromByteArray(buffer, size);
                //try
                //{
                    Message m = Message.Parser.ParseFrom(data);
                //Console.WriteLine("+\n{0}\n+", m);
                Thread.Sleep(2);
                MessageEventArgs args = new MessageEventArgs();
                    args.Message = m;
                    //Console.WriteLine("{0} TCP Got the message: {1}; Message data in bytes: {2}", _Index, size, DataBytesString(data));
                    string pipelineId=m.ToAbstractionId;
                string systemID = m.SystemId;
                if (m.NetworkMessage.Message.Type == Message.Types.Type.ProcInitializeSystem)
                    systemID = "base";
                PipelineExecutor systemExecutor = _Executors[systemID];
                //Stopwatch stopWatch = new Stopwatch();
                //stopWatch.Start();

                systemExecutor.ProcessMessageBottomUp(pipelineId, args);

               // w.WriteLine("{0} Message From: {1} Type: {2}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"), m.NetworkMessage.SenderListeningPort, m.NetworkMessage.Message.Type);
               // w.Flush();

                //stopWatch.Stop();
                //if (stopWatch.Elapsed > new TimeSpan(0, 0, 0, 0, 100))
                //{
                //Console.WriteLine(stopWatch.Elapsed);
                //}

                //lock (locker)
                //{
                //    eventQueue.Enqueue(m);
                //}
                //}
                //catch (Exception e)
                //{
                //    Console.WriteLine("{0} TCP Could not parse the message. Message size: {1}; Message data in bytes: {2}", _Index, size, DataBytesString(data));
                //}
                try
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                }
                catch (Exception e) { }
            }
        }

        public void TCPSend(object sender,MessageEventArgs messageArgs)
        {
            Message message = messageArgs.Message;
            message.NetworkMessage.SenderListeningPort = _MyEndPoint.Port;
            message.NetworkMessage.SenderHost = _MyEndPoint.Address.ToString();
            int endPort = messageArgs.EndPort;

            IPAddress destIP = System.Net.IPAddress.Parse(messageArgs.EndHost);

            IPEndPoint endPoint = new IPEndPoint(destIP, endPort);

            Socket sendingsocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sendingsocket.Connect(endPoint);
            //Console.WriteLine("{0} Sending Socket connected to -> {1} \n Message: {2} ", _Index,
            //              sendingsocket.RemoteEndPoint.ToString(),message.ToString());

            byte[] messageArray = message.ToByteArray();
            byte[] messageLengthBytes = BitConverter.GetBytes(messageArray.Length);
            byte[] fullMessage = CombineLengthAndData(messageLengthBytes, messageArray);
            //try
            //{
                int byteSent = sendingsocket.Send(fullMessage);
            //}
            //catch
            //{ }
            sendingsocket.Shutdown(SocketShutdown.Both);
            sendingsocket.Close();

          //  w.WriteLine("{0} Message TO: {1} Type: {2}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff"),endPort, message.NetworkMessage.Message.Type);
          //  w.Flush();
        }
        //private Queue<Message> _EventQueue;
        private Socket _ListeningSocket;
        private Socket _SendingSocket;
        private IPEndPoint _HubEndPoint;
        private IPEndPoint _MyEndPoint;
        private IPAddress _MyIP;
        private IPAddress _HubIP;
        private int _Index;
        private Dictionary<string,PipelineExecutor> _Executors;
        private static byte[] CombineLengthAndData(byte[] length, byte[] data)
        {
            byte[] output = new byte[length.Length + data.Length];
            for (int i = 0; i < 4; i++)
            {
                output[3 - i] = length[i];
            }
            Array.Copy(data, 0, output, length.Length, data.Length);
            return output;
        }
        private static int GetSizeFromByteArray(byte[] data)
        {
            byte[] output = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                output[3 - i] = data[i];
            }
            return BitConverter.ToInt32(output, 0);
        }
        private static byte[] GetDataFromByteArray(byte[] data, int size)
        {
            byte[] output = new byte[size];
            Array.Copy(data, 4, output, 0, output.Length);
            return output;
        }
        private static string DataBytesString(byte[] bytes)
        {
            string output = bytes[0].ToString();
            for (int i = 1; i < bytes.Length; i++)
                output += ":" + bytes[i].ToString();
            return output;
        }

    }
}
