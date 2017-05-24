using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashCam.Stream
{
    struct IPCamStreamClient
    {
        public System.IO.Stream ClientStream;
        public bool SentHeader;

        public IPCamStreamClient(System.IO.Stream client)
        {
            ClientStream = client;
            SentHeader = false;
        }

        internal void Sent()
        {
            SentHeader = true;
        }
    }
}
