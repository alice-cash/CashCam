using CashLib.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using CashCam.Module;
using CashLib.Collection;

namespace CashCam.Stream
{

    class IPCamStreamRepeater : IThreadTask
    {
        private List<IPCamStreamClient> clients = new List<IPCamStreamClient>();
        WebResponse WebResponse;
        System.IO.Stream requestStream;
        byte[] initalHeader;
        int HeaderState = 0;

        byte[] currentBlock;
        private IPCamStreamTask iPCamStreamTask;
        private string hostname;

        private System.Threading.Thread readThread;


        private LimitedStack<byte[]> ReadData;

        /*  public IPCamStreamRepeater()
        {

        }*/

        public IPCamStreamRepeater(IPCamStreamTask iPCamStreamTask, string hostname)
        {
            this.iPCamStreamTask = iPCamStreamTask;
            this.hostname = hostname;
            readThread = new System.Threading.Thread(Thread)
            { Name = "Read Thread for" + hostname };
            ReadData = new LimitedStack<byte[]>(20);
            readThread.Start();
        }

        public void AddStream(HttpListenerContext stream)
        {
            IPCamStreamClient client = new IPCamStreamClient(stream);

            try
            {
                ClientSendHeaderCheck(client);
            }
            catch { return; }
            clients.Add(client);
        }

        public void RunTask()
        {
            if (!iPCamStreamTask.Parent.StreamEnabled())
            { Stop(); return; }
            if (WebResponse == null || requestStream == null || !requestStream.CanRead)
            { Start(); return; }
            if (!requestStream.CanRead) return;

            lock (ReadData)
            {
                while (ReadData.Count > 0)
                {
                    byte[] data = ReadData.Pop();

                    SentToClients(data);
                    // Console.WriteLine("OUT");
                }
            }


        }

        private bool HeaderCheck(byte[] data)
        {
            if (HeaderState == 2) return true;

            if (HeaderState == 0)
                initalHeader = data;

            if (HeaderState > 1)
                initalHeader = AppendData(initalHeader, data);

            HeaderState++;
            return false;
        }


        private void Thread()
        {
            while (Program.ThreadsRunning)
            {
                if (iPCamStreamTask.Parent.StreamEnabled())
                {
                    if (WebResponse != null && requestStream != null && requestStream.CanRead)
                    {
                        try
                        {
                            byte[] data = new byte[128];
                            int count = requestStream.Read(data, 0, data.Length); ;
                            if (count == 0) continue;


                            data = TryGetBlock(data);

                            if (data.Length == 0) continue;

                            if (!HeaderCheck(data))
                                continue;

                            lock (ReadData)
                            {
                                ReadData.Push(data);
                            }
                            //Console.WriteLine("IN");
                        }
                        catch { }
                    }

                }

            }
        }


        private byte[] TryGetBlock(byte[] data)
        {
            //byte[] data = new byte[128];
            //int count = requestStream.Read(data, 0, data.Length);
            int count = data.Length;
            byte[] returnDataBlock;

            if (count == 0)
                return new byte[0];

            currentBlock = AppendData(currentBlock, data, count);

            int HeaderPos = GetOggSHeader(currentBlock, 1);

            // If there is no header then the current block is not finished.
            if (HeaderPos == -1)
                return new byte[0];

            // Append all of the data up to the next OggS to the previously saved data and send it out.
            returnDataBlock = new byte[HeaderPos];

            Array.Copy(currentBlock, returnDataBlock, HeaderPos);

            // Save all of the rest of the unsed data and shift it forward.
            byte[] tmpBlock = new byte[currentBlock.Length - HeaderPos];
            Array.Copy(currentBlock, HeaderPos, tmpBlock, 0, currentBlock.Length - HeaderPos);
            currentBlock = tmpBlock;

            //Console.WriteLine("========" + (returnDataBlock.Length + HeaderPos) + "================================================");
            //Console.WriteLine(BitConverter.ToString(workingBlock).Replace("-", ""));

            string s = ASCIIEncoding.ASCII.GetString(returnDataBlock, 0, returnDataBlock.Length);
            var sb = new StringBuilder();

            foreach (char c in s)
            {
                if (32 <= c && c <= 126)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(".");
                }
            }
            Console.WriteLine(sb.ToString().Substring(0, sb.ToString().Length < 512 ? sb.ToString().Length : 512));

            // return the previously clipped data
            return returnDataBlock;
        }


        private void SentToClients(byte[] data)
        {
            List<IPCamStreamClient> toRemove = new List<IPCamStreamClient>();

            foreach (IPCamStreamClient client in clients)
            {
                try
                {
                    if (ClientSendHeaderCheck(client))
                    {
                        client.ClientStream.Response.OutputStream.Write(data, 0, data.Length);
                        client.ClientStream.Response.OutputStream.Flush();
                    }

                }
                catch { toRemove.Add(client); }
            }
            foreach (IPCamStreamClient client in toRemove)
                clients.Remove(client);
        }

        private bool ClientSendHeaderCheck(IPCamStreamClient client)
        {
            if (client.SentHeader) return true;
            if (HeaderState != 2) return false;

            client.ClientStream.Response.OutputStream.Write(initalHeader, 0, initalHeader.Length);
            client.SentHeader = true;

            return true;
        }

        private byte[] AppendData(byte[] a, byte[] b)
        {
            return AppendData(a, b, b.Length);
        }

        private byte[] AppendData(byte[] a, byte[] b, int b_length)
        {
            byte[] newData = new byte[a.Length + b_length];
            Array.Copy(a, newData, a.Length);
            Array.Copy(b, 0, newData, a.Length, b_length);
            return newData;
        }

        private int GetOggSHeader(byte[] data, int offset = 0, int limit = -1)
        {
            if (limit == -1) limit = data.Length;
            //string s = ASCIIEncoding.ASCII.GetString(data, 0, limit);
            // return s.IndexOf("OggS", offset + 1);

            for (int i = offset; i < limit - 4; i++)
            {
                if (data[i] == 'O' && data[i + 1] == 'g' && data[i + 2] == 'g' && data[i + 3] == 'S')
                    return i;
                // OggS
            }
            return -1;
        }

        public void Start()
        {
            StopClients();
            if (WebResponse != null) WebResponse.Close();
            //  clients = new List<IPCamStreamClient>();
            HttpWebRequest request = HttpWebRequest.CreateHttp(hostname);
            try
            {
                WebResponse = request.GetResponse();
                requestStream = WebResponse.GetResponseStream();
            }
            catch { }
            currentBlock = new byte[0];
        }

        public void Stop()
        {
            StopClients();

            try { requestStream?.Close(); } catch { }
            try { WebResponse?.Close(); } catch { }
            requestStream = null;
            WebResponse = null;
        }

        private void StopClients()
        {
            if (clients != null)
                foreach (IPCamStreamClient client in clients)
                    try { client.ClientStream.Response.OutputStream.Close(); } catch { }
        }
    }
}
