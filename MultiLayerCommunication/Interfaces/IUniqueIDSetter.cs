using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLayerCommunication.Interfaces
{
    public interface IUniqueIDSetter
    {
        void Set(IAbstractionable layer, string[] ids);
    }
}
