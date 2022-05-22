using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLayerCommunication.Interfaces
{
    public interface ICommunicable
    {
        void Send(object sender, IMessageArgumentable messageArgs);
        void AddSystemExecutor(string systemID,PipelineExecutor executor);

    }
}
