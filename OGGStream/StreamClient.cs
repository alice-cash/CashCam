using CashCam.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebClient = CashCam.HTTP.WebClient;

namespace CashCam.OGGStream
{
    class StreamClient
    {
        public System.IO.Stream ClientStream;
        public bool SentHeader;
        public int ID { get; private set; }

        private static Random r = new Random();


        public StreamClient(System.IO.Stream stream)
        {
            ClientStream = stream;
            SentHeader = false;
            ID = r.Next();
        }
    }
}
