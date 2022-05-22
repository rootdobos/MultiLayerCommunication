using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiLayerCommunication.Interfaces;

namespace MultiLayerCommunication
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
        public Pipeline(IProcess process, IAbstractionable[] layers, ICommunicable tCPCommunicator)
        {
            _Process = process;
            _Layers = layers;
            _Communicator = tCPCommunicator;

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
                if (!_Layers[i].SubscribedToDeliver.Contains(_Layers[i -1]))
                {
                    _Layers[i].DeliverEvent += _Layers[i - 1].Deliver;
                    _Layers[i].SubscribedToDeliver.Add(_Layers[i - 1]);
                }
            }
            for (int i = 0; i < _Layers.Length-1; i++)
            {
                if(!_Layers[i].SubscribedToSend.Contains( _Layers[i+1]))
                {
                    _Layers[i].SendEvent += _Layers[i + 1].Send;
                    _Layers[i].SubscribedToSend.Add(_Layers[i + 1]);
                }
            }
            _Layers[_Layers.Length - 1].SendEvent += _Communicator.Send;
        }

        public void ProcessMessageBottomUp(IMessageArgumentable message)
        {
            _Layers[_Layers.Length - 1].Deliver(this,message);
        }
        public void ProcessMessageUpBottom(IMessageArgumentable message)
        {
            _Layers[0].Send(this,message);
        }
        private IAbstractionable[] _Layers;
        private IProcess _Process;
        private ICommunicable _Communicator;
    }
}
