using Google.Protobuf.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedSystem
{
    public class CommunicationSystem
    {
        public string SystemID;
        public List<ProcessId> Processes
        {
            set { _Executor.Processes = value; }
        }

        public ProcessId OwnerProcessId
        {
            set { _Executor.OwnerProcessID = value; }
        }
        public CommunicationSystem(string systemID, Process process, TCPCommunicator tcpcommunicator)
        {
            _OwnerProcess = process;
            _Executor = new PipelineExecutor(_OwnerProcess, tcpcommunicator);

            SystemID = systemID;
            _Executor.SystemID = SystemID;
            tcpcommunicator.AddSystemExecutor(SystemID, _Executor);
        }
        public void Send(string pipelineID, MessageEventArgs message)
        {
            _Executor.ProcessMessageUpBottom(pipelineID,message);
        }
        Process _OwnerProcess;
        private PipelineExecutor _Executor;

    }
}
