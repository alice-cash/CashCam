using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashCam.Stream
{
    class IPCamStreamClient
    {
        public System.IO.Stream ClientStream;
        public bool SentHeader;
        public int ID { get; private set; }

        private static Random r = new Random();


        public IPCamStreamClient(System.IO.Stream client)
        {
            ClientStream = client;
            SentHeader = false;
            ID = r.Next();
        }
    }
}
