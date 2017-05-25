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

        private List<WebClient> clients = new List<WebClient>();
        private List<WebClient> toRemove = new List<WebClient>();

        public WebServer()
        {
            
        }


        public void RunTask()
        {
            if (!_listener.IsListening) return;


            
            var asynccontext = _listener.BeginGetContext((IAsyncResult result) =>
            {
                HttpListenerContext context;
                try
                {
                    context = _listener.EndGetContext(result);
                    clients.Add(new WebClient(context));
                }
                catch {  }

            }, _listener);

            do
            {
                if (!Program.ThreadsRunning) return;

                toRemove.Clear();
                foreach (WebClient client in clients)
                    if (client.Poll())
                        toRemove.Add(client);
                foreach (WebClient client in toRemove)
                    clients.Remove(client);

                System.Threading.Thread.Yield();
            } while (asynccontext.AsyncWaitHandle.WaitOne(10) && !asynccontext.IsCompleted);

        }



        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:8080/");
            _listener.Start();
        }

        public void Stop()
        {
            if (_listener.IsListening)
            {
                _listener.Stop();
            }
        }
    }
}
