using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Communication;
namespace DistributedSystem
{
    public class PipelineExecutor
    {

        public List<ProcessId> Processes;
        public string SystemID;
        public ProcessId OwnerProcessID;
        public Dictionary<string, Pipeline> Pipelines
        { get { return _Pipelines; } }
        public Dictionary<string, IAbstractionable> UniqueDependencyLayers
        {
            get { return _UniqueDependencyLayers; }
        }
        public void ProcessMessageBottomUp(string pipelineID, MessageEventArgs message)
        {
            if (!_Pipelines.ContainsKey(pipelineID))
            {
                if (!Utilities.IsElementInString(pipelineID, _InternalCreatables))
                {
                    GeneratePipeline(pipelineID);
                    CheckAdditionalPipelines(pipelineID);
                }
            }
            if(_Pipelines.ContainsKey(pipelineID))
                _Pipelines[pipelineID].ProcessMessageBottomUp(message);
        }
        public void ProcessMessageUpBottom(string pipelineID, MessageEventArgs message)
        {
            if (!_Pipelines.ContainsKey(pipelineID))
            {
                if (!Utilities.IsElementInString(pipelineID, _InternalCreatables))
                {
                    GeneratePipeline(pipelineID);
                    CheckAdditionalPipelines(pipelineID);
                }
            }
            if (_Pipelines.ContainsKey(pipelineID))
                _Pipelines[pipelineID].ProcessMessageUpBottom(message);
        }

        public PipelineExecutor(Process process, TCPCommunicator tcpCommunicator)
        {
            _App = process;
            _Pipelines = new Dictionary<string, Pipeline>();
            _UniqueLayers = new Dictionary<string, IAbstractionable>();
           
            _UniqueDependencyLayers = new Dictionary<string, IAbstractionable>();
            _TCPCommunicator = tcpCommunicator;

            _InternalCreatables = new List<string>();
            _InternalCreatables.Add("ep[");

            GenerateInitialPipeline();
        }

        private void GenerateInitialPipeline()
        {
            IAbstractionable[] layers = new IAbstractionable[1];
            PerfectLink pl = new PerfectLink();
            layers[0] = pl;
            Pipeline app_pl = new Pipeline(_App, layers, _TCPCommunicator);
            //app_pl.Layers[app_pl.Layers.Length - 1].SendEvent += tcpCommunicator.TCPSend;
            _Pipelines.Add("app.pl", app_pl);
        }

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
                    layers[i - 1] = AbstractionFactory.Produce(layersIDs[i]);
                    AddUnique(layersIDs[i], layers[i - 1]);
                    RunInit(layers[i - 1], layersIDs[i]);
                    SetUniqueID(layers[i - 1],layersIDs);
                }
            }
            Pipeline newPipeline = new Pipeline(_App, layers, _TCPCommunicator);
            _Pipelines.Add(id, newPipeline);
        }



        public void RunInit(IAbstractionable layer,string id)
        {
            if (id == "beb")
            ((BestEffortBroadcast)layer).Init(Processes);
            else if (id.Contains("nnar"))
            {
                ((NNAtomicRegister)layer).Init(Processes,OwnerProcessID.Rank,SystemID);
            }
            else if(id.Contains("ec"))
            {
                ((EpochChange)layer).Init(Utilities.MaxRank(Processes), OwnerProcessID, Processes.Count, SystemID);
            }
            else if(id.Contains("eld"))
            {
                ((EventualLeaderDetector)layer).Init(Processes,SystemID);
            }
            else if(id.Contains("epfd"))
            {
                ((EventuallyPerfectFailureDetector)layer).Init(Processes, SystemID);
            }
            else if(id.Contains("uc"))
            {
                ((UniformConsensus)layer).Init(Utilities.MaxRank(Processes), OwnerProcessID,Processes.Count,this,SystemID);
            }
        }

        private void SetUniqueID(IAbstractionable layer,string[] ids)
        {
            if(layer is EventuallyPerfectFailureDetector)
            {
                string id = "";
                int i = 0;
                while(ids[i]!="epfd")
                {
                    id += ids[i]+".";
                    i++;
                }
                id += "epfd";

                ((EventuallyPerfectFailureDetector)layer).MyID = id;
            }
        }
        private void CheckAdditionalPipelines(string id)
        {
            string[] layersIDs = id.Split('.');
            foreach (string layerID in layersIDs)
            {
                if (layerID.Contains("nnar"))
                {
                    string additionalID = "app." + layerID + ".pl";
                    if (!_Pipelines.ContainsKey(additionalID))
                        GeneratePipeline(additionalID);
                    additionalID= "app." + layerID+".beb" + ".pl";
                    if (!_Pipelines.ContainsKey(additionalID))
                        GeneratePipeline(additionalID);
                }
                else if(layerID.Contains("uc"))
                {
                    

                    string additionalID = "app." + layerID+ ".ec"+".eld" +".epfd"+  ".pl";
                    if (!_Pipelines.ContainsKey(additionalID))
                        GeneratePipeline(additionalID);
                    additionalID = "app." + layerID+ ".ec" + ".beb" + ".pl";
                    if (!_Pipelines.ContainsKey(additionalID))
                        GeneratePipeline(additionalID);

                    additionalID = "app." + layerID + ".ec" + ".pl";
                    if (!_Pipelines.ContainsKey(additionalID))
                        GeneratePipeline(additionalID);
                }
            }
        }
        private void AddUnique(string layerID, IAbstractionable layer)
        {
            if(layerID.Contains("nnar") ||
                layerID.Contains("uc"))
                _UniqueLayers.Add(layerID, layer);

        }

        private List<string> _InternalCreatables;
        private Dictionary<string, Pipeline> _Pipelines;
        private Dictionary<string, IAbstractionable> _UniqueLayers;

        private Dictionary<string, IAbstractionable> _UniqueDependencyLayers;
        private Process _App;
        private TCPCommunicator _TCPCommunicator;
    }
}
