using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiLayerCommunication.Interfaces;
namespace MultiLayerCommunication
{
    public class PipelineExecutor
    {

       // public List<ProcessId> Processes;
        public string SystemID;
       // public ProcessId OwnerProcessID;
        public Dictionary<string, Pipeline> Pipelines
        { get { return _Pipelines; } }
        public Dictionary<string, IAbstractionable> UniqueDependencyLayers
        {
            get { return _UniqueDependencyLayers; }
        }
        public AbstractionFactory Factory
        {
            get { return _Factory; }
        }
        public void ProcessMessageBottomUp(string pipelineID, IMessageArgumentable message)
        {
            if (!_Pipelines.ContainsKey(pipelineID))
            {
                if (!IsElementInString(pipelineID, _Descriptor.InternalCreatables))
                {
                    GeneratePipeline(pipelineID);
                    CheckAdditionalPipelines(pipelineID);
                }
            }
            if(_Pipelines.ContainsKey(pipelineID))
                _Pipelines[pipelineID].ProcessMessageBottomUp(message);
        }
        public void ProcessMessageUpBottom(string pipelineID, IMessageArgumentable message)
        {
            if (!_Pipelines.ContainsKey(pipelineID))
            {
                if (!IsElementInString(pipelineID, _Descriptor.InternalCreatables))
                {
                    GeneratePipeline(pipelineID);
                    CheckAdditionalPipelines(pipelineID);
                }
            }
            if (_Pipelines.ContainsKey(pipelineID))
                _Pipelines[pipelineID].ProcessMessageUpBottom(message);
        }
       
        public PipelineExecutor(IProcess process, ICommunicable tcpCommunicator, AbstractionFactory factory, PipelineExecutorDescriptor descriptor)
        {
            _Process = process;
            _Factory = factory;
            _Descriptor = descriptor;

            _Pipelines = new Dictionary<string, Pipeline>();
            _UniqueLayers = new Dictionary<string, IAbstractionable>();

            _UniqueDependencyLayers = new Dictionary<string, IAbstractionable>();
            _TCPCommunicator = tcpCommunicator;

            //_InternalCreatables.Add("ep[");   //need in the PoC

            // GenerateInitialPipeline();
        }

        //private void GenerateInitialPipeline()
        //{
        //    IAbstractionable[] layers = new IAbstractionable[1];
        //    PerfectLink pl = new PerfectLink();
        //    layers[0] = pl;
        //    Pipeline app_pl = new Pipeline(_Process, layers, _TCPCommunicator);
        //    //app_pl.Layers[app_pl.Layers.Length - 1].SendEvent += tcpCommunicator.TCPSend;
        //    _Pipelines.Add("app.pl", app_pl);
        //}

        public void GeneratePipeline(string id)
        {
            string[] layersIDs= id.Split('.');
            IAbstractionable[] layers = new IAbstractionable[layersIDs.Length - 1];
            for(int i=1;i<layersIDs.Length;i++)
            {
                if (_UniqueLayers.ContainsKey(layersIDs[i]))
                    layers[i - 1] = _UniqueLayers[layersIDs[i]];
                else if (_UniqueDependencyLayers.ContainsKey(layersIDs[i - 1] + "." + layersIDs[i]))
                    layers[i - 1] = _UniqueDependencyLayers[layersIDs[i - 1] + "." + layersIDs[i]];
                else
                {
                    layers[i - 1] = _Factory.Produce(layersIDs[i]);
                    AddUnique(layersIDs[i], layers[i - 1]);
                    RunInit(layers[i - 1], layersIDs[i]);
                    SetUniqueID(layers[i - 1],layersIDs);
                }
            }
            Pipeline newPipeline = new Pipeline(_Process, layers, _TCPCommunicator);
            _Pipelines.Add(id, newPipeline);
        }



        public void RunInit(IAbstractionable layer,string id)
        {
            foreach(IInitializer initer in _Descriptor.Initializers)
            {
                if (id.Contains(initer.InitableID))
                {
                    initer.Init(layer);
                    break;
                }
            }
        }

        private void SetUniqueID(IAbstractionable layer,string[] ids)
        {
            foreach(IUniqueIDSetter setter in _Descriptor.UniqueIDSetters)
            {
                setter.Set(layer, ids);
            }
        }
        private void CheckAdditionalPipelines(string id)
        {
            string[] layersIDs = id.Split('.');
            foreach (string layerID in layersIDs)
            {
                foreach(IAdditionalPipelineContainable container in _Descriptor.AdditionalPipelineNameContainers)
                {
                    if(layerID.Contains(container.BaseID))
                    {
                        foreach(string pipelineId in container.AdditionalPipelines(layerID))
                        {
                            if (!_Pipelines.ContainsKey(pipelineId))
                                GeneratePipeline(pipelineId);
                        }
                    }
                }
            }
        }
        private void AddUnique(string layerID, IAbstractionable layer)
        {
            foreach(string unique in _Descriptor.UniqueLayerNames)
            {
                if(layerID.Contains(unique))
                    _UniqueLayers.Add(layerID, layer);
            }
        }

        private static bool IsElementInString(string str, List<string> list)
        {
            foreach (string element in list)
                if (str.Contains(element))
                    return true;
            return false;
        }

       

        private Dictionary<string, Pipeline> _Pipelines;
        private Dictionary<string, IAbstractionable> _UniqueLayers;
        private AbstractionFactory _Factory;
        private Dictionary<string, IAbstractionable> _UniqueDependencyLayers;
        private IProcess _Process;
        private ICommunicable _TCPCommunicator;
        private PipelineExecutorDescriptor _Descriptor;
    }
}
