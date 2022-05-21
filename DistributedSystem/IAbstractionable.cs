using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Communication;
namespace DistributedSystem
{
    public interface IAbstractionable
    {

        event EventHandler<MessageEventArgs> DeliverEvent;
        event EventHandler<MessageEventArgs> SendEvent;

        //void Send(Message message);
        void Send(object sender,MessageEventArgs messageArgs);
        void Deliver(object sender,MessageEventArgs messageArgs);
        //void Deliver(Message message);
    }
}
