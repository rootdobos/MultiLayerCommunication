using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Communication;

namespace DistributedSystem
{
    public class Pipeline
    {
        public IAbstractionable[] Layers
        {
            get
            {
                return _Layers;
            }
        }
        public Pipeline(Process process, IAbstractionable[] layers, TCPCommunicator tCPCommunicator)
        {
            _Process = process;
            _Layers = layers;
            _TCPCommunicator = tCPCommunicator;

            InitSubscriptions();
            //_Layers[0].DeliverEvent += _Process.SubscribeToDeliver;


            //_Layers[_Layers.Length - 1].SendEvent += _TCPCommunicator.TCPSend;
        }

        public void InitSubscriptions()
        {
            if (!_Process.Subscribed.Contains(_Layers[0]))
            {
                _Layers[0].DeliverEvent += _Process.SubscribeToDeliver;
                _Process.Subscribed.Add(_Layers[0]);
            }
            for (int i=1; i<_Layers.Length;i++)
            {
                _Layers[i].DeliverEvent += _Layers[i - 1].Deliver;
            }
            for (int i = 0; i < _Layers.Length-1; i++)
            {
                if(_Layers[i+1] is EpochConsensus)
                {
                    if(((EpochConsensus)_Layers[i+1]).SubscribedToSend==false)
                    {
                        _Layers[i].SendEvent += _Layers[i + 1].Send;
                       ( (EpochConsensus)_Layers[i + 1]).SubscribedToSend = true;
                    }
                }
                else
                _Layers[i].SendEvent += _Layers[i+1].Send;
            }
            _Layers[_Layers.Length - 1].SendEvent += _TCPCommunicator.TCPSend;
        }

        public void ProcessMessageBottomUp(MessageEventArgs message)
        {
            _Layers[_Layers.Length - 1].Deliver(this,message);
        }
        public void ProcessMessageUpBottom(MessageEventArgs message)
        {
            _Layers[0].Send(this,message);
        }
        private IAbstractionable[] _Layers;
        private Process _Process;
        private TCPCommunicator _TCPCommunicator;
    }
}
