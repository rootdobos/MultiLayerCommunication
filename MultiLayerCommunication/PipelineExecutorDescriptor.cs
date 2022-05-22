using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiLayerCommunication.Interfaces;

namespace MultiLayerCommunication
{
    public class PipelineExecutorDescriptor
    {
        public List<string> InternalCreatables
        {
            get
            {
                return _InternalCreatables;
            }
        }

        public List<string> UniqueLayerNames
        {
            get
            {
                return _UniqueLayerNames;
            }
        }

        public List<IUniqueIDSetter> UniqueIDSetters
        {
            get
            {
                return _UniqueIDSetters;
            }
        }
        public List<IAdditionalPipelineContainable> AdditionalPipelineNameContainers
        {
            get
            {
                return _AdditionalPipelineNameContainers;
            }
        }

        public List<IInitializer> Initializers
        {
            get
            {
                return _Initializers;
            }
        }

        public void AddInternalCreatables(IEnumerable<string> internalcreatables)
        {
            _InternalCreatables.AddRange(internalcreatables);
        }
        public void AddUniqueLayersName(IEnumerable<string> layersnames)
        {
            _UniqueLayerNames.AddRange(layersnames);
        }
        public void AddInitializers(IEnumerable<IInitializer> initializers)
        {
            _Initializers.AddRange(initializers);
        }
        public void AddAdditionalPipelineNames(IEnumerable<IAdditionalPipelineContainable> names)
        {
            _AdditionalPipelineNameContainers.AddRange(names);
        }
        public PipelineExecutorDescriptor()
        {
            _UniqueLayerNames = new List<string>();
            _AdditionalPipelineNameContainers = new List<IAdditionalPipelineContainable>();

            _InternalCreatables = new List<string>();
            _UniqueIDSetters = new List<IUniqueIDSetter>();
            _Initializers = new List<IInitializer>();
        }
        private List<string> _InternalCreatables;
        private List<IUniqueIDSetter> _UniqueIDSetters;
        private List<string> _UniqueLayerNames;
        private List<IAdditionalPipelineContainable> _AdditionalPipelineNameContainers;

        private List<IInitializer> _Initializers;
    }
}
