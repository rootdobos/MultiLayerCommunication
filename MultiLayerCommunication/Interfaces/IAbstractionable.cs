using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MultiLayerCommunication.Interfaces
{
    public interface IAbstractionable
    {

        event EventHandler<IMessageArgumentable> DeliverEvent;
        event EventHandler<IMessageArgumentable> SendEvent;

        List<object> SubscribedToSend { get; }
        List<object> SubscribedToDeliver { get; }

        //void Send(Message message);
        void Send(object sender, IMessageArgumentable messageArgs);
        void Deliver(object sender, IMessageArgumentable messageArgs);
        //void Deliver(Message message);
    }
}
