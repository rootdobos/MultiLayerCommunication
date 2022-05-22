
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiLayerCommunication.Interfaces;
namespace MultiLayerCommunication
{
    public class CommunicationSystem
    {
        public string SystemID;
        //public PipelineExecutor Executor { get { return Executor; } }
        public CommunicationSystem(string systemID, IProcess process, ICommunicable communicator, AbstractionFactory factory , PipelineExecutorDescriptor descriptor)
        {
            _OwnerProcess = process;
            _Executor = new PipelineExecutor(_OwnerProcess, communicator, factory, descriptor);

            SystemID = systemID;
            _Executor.SystemID = SystemID;
            communicator.AddSystemExecutor(SystemID, _Executor);
        }
        public void Send(string pipelineID, IMessageArgumentable message)
        {
            _Executor.ProcessMessageUpBottom(pipelineID,message);
        }
        IProcess _OwnerProcess;
        private PipelineExecutor _Executor;

    }
}
