using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Communication;

namespace DistributedSystem
{
    public class EpochChange : IAbstractionable
    {

        public static readonly string MyID = "ec";

        public event EventHandler<MessageEventArgs> DeliverEvent;
        public event EventHandler<MessageEventArgs> SendEvent;
        public EpochChange()
        {

        }
        public void Init(ProcessId l0, ProcessId self, int n, string systemID)
        {
            _Trusted = l0;
            _Lastts = 0;
            _Self = self;
            _Ts = _Self.Rank;
            _NumberOfProcesses = n;
            _SystemID = systemID;
        }
        public void Deliver(object sender, MessageEventArgs messageArgs)
        {
            Message message = messageArgs.Message;
            if(message.Type== Message.Types.Type.EldTrust && Utilities.IsMyMessage(message.ToAbstractionId, MyID))
            {
                _Trusted = message.EldTrust.Process;
                if (Utilities.AreEqualProcesses(_Self,_Trusted))
                {
                    _Ts += _NumberOfProcesses;
                    BebBroadcastNewEpoch();
                }
            }
            else if (message.Type== Message.Types.Type.BebDeliver && Utilities.IsMyMessage(message.ToAbstractionId, MyID))
            {
                if(message.BebDeliver.Message.Type== Message.Types.Type.EcInternalNewEpoch )
                {
                    if(Utilities.AreEqualProcesses(message.BebDeliver.Sender, _Trusted) && message.BebDeliver.Message.EcInternalNewEpoch.Timestamp>_Lastts)
                    {
                        _Lastts = message.BebDeliver.Message.EcInternalNewEpoch.Timestamp;
                        StartEpoch(message.BebDeliver.Message.EcInternalNewEpoch.Timestamp);
                    }
                    else
                    {
                        SendNack(message.BebDeliver.Sender, message.ToAbstractionId);
                    }
                }
            }
            else if(message.Type== Message.Types.Type.PlDeliver && Utilities.IsMyMessage(message.ToAbstractionId, MyID))
            {
                if (message.PlDeliver.Message.Type == Message.Types.Type.EcInternalNack)
                {
                    if (Utilities.AreEqualProcesses(_Self, _Trusted))
                    {
                        _Ts += _NumberOfProcesses;
                        BebBroadcastNewEpoch();
                    }
                }
            }
        }
        public void StartEpoch(int newTS)
        {
            Message message = new Message();

            message.Type = Message.Types.Type.EcStartEpoch;
            message.EcStartEpoch = new EcStartEpoch();
            message.EcStartEpoch.NewLeader = _Trusted.Clone();
            message.EcStartEpoch.NewTimestamp = newTS;


            EventHandler<MessageEventArgs> handler = DeliverEvent;
            MessageEventArgs args = new MessageEventArgs();
            args.Message = message;
            handler?.Invoke(this, args);
        }
        public void SendNack(ProcessId l, string toAbstraction)
        {
            Message message = new Message();

            message.Type = Message.Types.Type.PlSend;
            message.FromAbstractionId = "app." + MyID;
            message.ToAbstractionId = toAbstraction + ".pl";
            message.SystemId = _SystemID;
            message.PlSend = new PlSend();
            message.PlSend.Message = new Message();
            message.PlSend.Message.FromAbstractionId = toAbstraction;
            message.PlSend.Message.ToAbstractionId = toAbstraction;
            message.PlSend.Message.Type = Message.Types.Type.EcInternalNack;


            message.PlSend.Destination = new ProcessId();
            message.PlSend.Destination.Host = l.Host;
            message.PlSend.Destination.Port = l.Port;

            message.PlSend.Message.EcInternalNack = new EcInternalNack();

            EventHandler<MessageEventArgs> handler = SendEvent;
            MessageEventArgs args = new MessageEventArgs();
            args.Message = message;
            handler?.Invoke(this, args);
        }
        public void BebBroadcastNewEpoch()
        {
            Message message = new Message();

            message.Type = Message.Types.Type.BebBroadcast;
            message.FromAbstractionId = "app." + MyID;
            message.ToAbstractionId = "app." + MyID + ".beb.pl";
            message.SystemId = _SystemID;
            message.BebBroadcast = new BebBroadcast();
            message.BebBroadcast.Message = new Message();
            message.BebBroadcast.Message.FromAbstractionId = "app." + MyID;
            message.BebBroadcast.Message.ToAbstractionId = "app." + MyID;
            message.BebBroadcast.Message.Type = Message.Types.Type.EcInternalNewEpoch;

            message.BebBroadcast.Message.EcInternalNewEpoch = new EcInternalNewEpoch();
            message.BebBroadcast.Message.EcInternalNewEpoch.Timestamp = _Ts;

            EventHandler<MessageEventArgs> handler = SendEvent;
            MessageEventArgs args = new MessageEventArgs();
            args.Message = message;
            handler?.Invoke(this, args);
        }

        public void Send(object sender, MessageEventArgs messageArgs)
        {
            EventHandler<MessageEventArgs> handler = SendEvent;
            MessageEventArgs args = messageArgs;
            //args.Message = message;
            handler?.Invoke(this, args);
        }
        ProcessId _Trusted;
        int _Lastts;
        int _Ts;
        int _NumberOfProcesses;
        ProcessId _Self;
        string _SystemID;
    }
}
