using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Communication;
namespace DistributedSystem
{
    public class MessageEventArgs:EventArgs
    {
        public Message Message { get; set; }
        public string EndHost { get; set; }
        public int EndPort { get; set; }
    }
}
