using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Communication;
namespace DistributedSystem
{
    public class PerfectLink : IAbstractionable
    {
        public static readonly string MyID = "pl";

        public event EventHandler<MessageEventArgs> DeliverEvent;
        public event EventHandler<MessageEventArgs> SendEvent;
        //public void Send(Message message)
        //{
        //    message.FromAbstractionId = Utilities.AddMyAbstractionId(message.FromAbstractionId, MyID);
        //    EventHandler<MessageEventArgs> handler = SendEvent;
        //    MessageEventArgs args = new MessageEventArgs();
        //    args.Message = message;
        //    handler?.Invoke(this, args);
        //}
        public void Send(object sender, MessageEventArgs messageArgs)
        {
            Message message = messageArgs.Message;
            if (message.Type == Message.Types.Type.PlSend)
            {
                string abstractionID = message.FromAbstractionId;


                Message m = new Message();
                m.Type = Message.Types.Type.NetworkMessage;
                m.FromAbstractionId= Utilities.AddMyAbstractionId(abstractionID, MyID);
                m.ToAbstractionId = message.ToAbstractionId;
                m.SystemId = message.SystemId;
                m.NetworkMessage = new NetworkMessage();
                m.NetworkMessage.Message = message.PlSend.Message;
                m.NetworkMessage.Message = message.PlSend.Message;
                m.NetworkMessage.Message = message.PlSend.Message;

                EventHandler<MessageEventArgs> handler = SendEvent;
                MessageEventArgs args = new MessageEventArgs();
                args.Message = m;
                args.EndHost = message.PlSend.Destination.Host;
                args.EndPort = message.PlSend.Destination.Port; 
                handler?.Invoke(this, args);
            }
        }
        //public void Deliver(Message message)
        //{
        //    message.ToAbstractionId = Utilities.RemoveMyAbstractionId(message.ToAbstractionId, MyID);
        //    EventHandler<MessageEventArgs> handler = DeliverEvent;
        //    MessageEventArgs args = new MessageEventArgs();
        //    args.Message = message;
        //    handler?.Invoke(this, args);
        //}

        public void Deliver(object sender,MessageEventArgs messageArgs)
        {
            Message message = messageArgs.Message;
            if (message.Type == Message.Types.Type.NetworkMessage && Utilities.IsMyMessage(message.ToAbstractionId,MyID))
            {
                Message m = new Message();
                m.Type = Message.Types.Type.PlDeliver;
                m.ToAbstractionId = Utilities.RemoveMyAbstractionId(message.ToAbstractionId, MyID);
                m.FromAbstractionId = message.FromAbstractionId;
                m.SystemId = message.SystemId;
                m.PlDeliver = new PlDeliver();
                m.PlDeliver.Sender = new ProcessId();
                m.PlDeliver.Sender.Host = message.NetworkMessage.SenderHost;
                m.PlDeliver.Sender.Port = message.NetworkMessage.SenderListeningPort;
                m.PlDeliver.Message = new Message();
                m.PlDeliver.Message = message.NetworkMessage.Message;

                //message.ToAbstractionId = Utilities.RemoveMyAbstractionId(message.ToAbstractionId,MyID);
                EventHandler<MessageEventArgs> handler = DeliverEvent;
                MessageEventArgs args = new MessageEventArgs();
                args.Message = m;
                handler?.Invoke(this, args);
            }
        }
        //public static void TCPDeliver(Socket listeningSocket, IPEndPoint ip, List<Message> eventQueue)
        

        //private TcpClient _Client;
       
    }
}
