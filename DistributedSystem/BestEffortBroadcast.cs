using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Communication;
namespace DistributedSystem
{
    public class BestEffortBroadcast : IAbstractionable
    {
        public static readonly string MyID = "beb";

        public event EventHandler<MessageEventArgs> DeliverEvent;
        public event EventHandler<MessageEventArgs> SendEvent;
        public BestEffortBroadcast()
        {

        }
        public BestEffortBroadcast(List<ProcessId> processes)
        {
            Init(processes);
            //_PerfectLink = perfectLink;
            //_PerfectLink.Deliver += DeliverFunction;
        }
        public void Init(List<ProcessId> processes )
        {
            _Processes = processes;
        }
        //public void Send(Message message)
        //{
        //    EventHandler<MessageEventArgs> handler = SendEvent;
        //    MessageEventArgs args = new MessageEventArgs();
        //    args.Message = message;
        //    handler?.Invoke(this, args);
        //}
        public void Deliver(object sender, MessageEventArgs messageArgs)
        {
            Message message = messageArgs.Message;
            if (message.Type == Message.Types.Type.PlDeliver && Utilities.IsMyMessage(message.ToAbstractionId, MyID))
            {
                Message m = new Message();
                m.Type = Message.Types.Type.BebDeliver;
                m.ToAbstractionId = Utilities.RemoveMyAbstractionId(message.ToAbstractionId, MyID);
                m.FromAbstractionId = message.FromAbstractionId;
                m.SystemId = message.SystemId;
                m.BebDeliver = new BebDeliver();
                m.BebDeliver.Sender = message.PlDeliver.Sender;
                m.BebDeliver.Message = new Message();
                m.BebDeliver.Message = message.PlDeliver.Message;

                EventHandler<MessageEventArgs> handler = DeliverEvent;
                MessageEventArgs args = new MessageEventArgs();
                args.Message = m;
                handler?.Invoke(this, args);
            }
        }

        public void Send(object sender,MessageEventArgs messageArgs)
        {
            Message message = messageArgs.Message;

            if (message.Type == Message.Types.Type.BebBroadcast)
                Broadcast(message);

            //EventHandler<MessageEventArgs> handler = SendEvent;
            //MessageEventArgs args = new MessageEventArgs();
            //args.Message = message;
            //handler?.Invoke(this, args);
        }

        public void Broadcast(Message message)
        {
            foreach (ProcessId process in _Processes)
            {
                Message m = new Message();
                m.Type = Message.Types.Type.PlSend;
                m.PlSend = new PlSend();
                m.PlSend.Destination = process;
                m.FromAbstractionId = Utilities.AddMyAbstractionId(message.FromAbstractionId, MyID);
                m.ToAbstractionId = message.ToAbstractionId;
                m.SystemId = message.SystemId;
                m.PlSend.Message = message.BebBroadcast.Message;

                EventHandler<MessageEventArgs> handler = SendEvent;
                MessageEventArgs args = new MessageEventArgs();
                args.Message = m;
                handler?.Invoke(this, args);
            }
        }
        //public void Deliver(Message message)
        //{
        //    throw new NotImplementedException();
        //}

        List<ProcessId> _Processes;
        PerfectLink _PerfectLink;

    }
}
