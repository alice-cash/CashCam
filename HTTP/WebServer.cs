using CashLib.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CashCam.HTTP
{
    class WebServer : IThreadTask
    {
        private HttpListener _listener = new HttpListener();



        public WebServer()
        {
            
        }


        public void RunTask()
        {
            if (!_listener.IsListening) return;

            HttpListenerContext context = _listener.GetContext();
            Program.WebClientManager.Add(new WebClient(context));
        }



        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:8080/");
            _listener.Start();
        }

        public void Stop(bool force)
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
            }            
        }
    }
}
