using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLayerCommunication
{
    public  class AbstractionFactory
    {

        public AbstractionFactory()
        { _Creators = new Dictionary<string, Interfaces.IAbstractionCreatable>(); }

        public void RegisterCreator(string id, Interfaces.IAbstractionCreatable creator)
        {
            _Creators.Add(id, creator);
        }

        public  Interfaces.IAbstractionable Produce(string id)
        {
            foreach(KeyValuePair<string, Interfaces.IAbstractionCreatable> kvPair in _Creators)
            {
                if(id.Contains(kvPair.Key))
                {
                    kvPair.Value.ID = id;
                    return  kvPair.Value.Create();
                }
            }
            return null;
        }

        Dictionary<string, Interfaces.IAbstractionCreatable> _Creators;
    }
}
