using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedSystem
{
    public static class AbstractionFactory
    {
        public static IAbstractionable Produce(string id)
        {
            if (id == "pl")
                return new PerfectLink();
            else if (id == "beb")
                return new BestEffortBroadcast();
            else if (id.Contains("nnar"))
            {
                string register = Utilities.GetRegisterName(id);
                return new NNAtomicRegister(register);
            }
            else if (id.Contains("epfd"))
                return new EventuallyPerfectFailureDetector();
            else if (id.Contains("eld"))
                return new EventualLeaderDetector();
            else if (id.Contains("ec"))
                return new EpochChange();
            else if(id.Contains("uc"))
            {
                string topic = Utilities.GetNameInParantheses(id);
                return new UniformConsensus(topic);
            }
            else if(id.Contains("ep"))
            {
                string ets= Utilities.GetNameInParantheses(id);
                return new EpochConsensus(ets);
            }
            return null;
        }
    }
}
