using System;
using CashLib.Threading;
using System.Collections.Generic;

namespace CashCam.HTTP
{
    internal class WebClientManager : IThreadTask
    {

        private List<WebClient> clients = new List<WebClient>();
        private List<WebClient> toRemove = new List<WebClient>();



        public void RunTask()
        {
            toRemove.Clear();

            // We want to avoid calling external code while locked.
            // To do so we generate a local copy and proceed from there.

            WebClient[] pollList;
            lock (clients)
            {
                pollList = new WebClient[clients.Count];
                clients.CopyTo(pollList);
            }

            foreach (WebClient client in pollList)
                if (client.Poll())
                    toRemove.Add(client);

            if (toRemove.Count == 0) return;

            lock (clients)
            {
                foreach (WebClient client in toRemove)
                {
                    clients.Remove(client);
                    Console.WriteLine("Removed");
                }
            }
        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        [ThreadSafe(ThreadSafeFlags.ThreadSafe)]
        internal void Add(WebClient webClient)
        {
            lock (clients)
            {
                Console.WriteLine("Added");
                clients.Add(webClient);
            }
        }
    }
}