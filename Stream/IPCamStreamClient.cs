using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CashCam.Stream
{
    class IPCamStreamClient
    {
        public HttpListenerContext ClientStream;
        public bool SentHeader;
        public int ID { get; private set; }

        private static Random r = new Random();


        public IPCamStreamClient(HttpListenerContext client)
        {
            ClientStream = client;
            SentHeader = false;
            ID = r.Next();
        }
    }
}
