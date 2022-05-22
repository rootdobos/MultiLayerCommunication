using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLayerCommunication.Interfaces
{
    public interface IProcess
    {
        List<object> Subscribed { get; }
        List<object> Subscriptions { get; }

        void SubscribeToDeliver(object sender, IMessageArgumentable args);

    }
}
