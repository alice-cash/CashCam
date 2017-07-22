using CashCam.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebClient = CashCam.HTTP.WebClient;

namespace CashCam.Stream
{
    class IPCamStreamClient
    {
        public WebClient WebClient;
        public HttpListenerContext ClientStream;
        public bool SentHeader;
        public int ID { get; private set; }

        private static Random r = new Random();


        public IPCamStreamClient(WebClient webClient)
        {
            WebClient = webClient;
            ClientStream = WebClient.Context;
            SentHeader = false;
            ID = r.Next();
        }
    }
}
