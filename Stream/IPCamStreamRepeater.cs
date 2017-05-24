using CashLib.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using CashCam.Module;

namespace CashCam.Stream
{

    class IPCamStreamRepeater : IThreadTask
    {
        private List<IPCamStreamClient> clients;
        WebResponse WebResponse;
        System.IO.Stream requestStream;
        byte[] initalHeader;
        int HeaderState = 0;
        
        byte[] currentBlock;

        public IPCamStreamRepeater()
        {
            Program.CameraRepater = this;
        }

        public void AddStream(System.IO.Stream stream)
        {
            clients.Add(new IPCamStreamClient(stream));
        }

        public void RunTask()
        {
            Console.WriteLine("DING");

            if (WebResponse == null || requestStream == null) return;
            if (!requestStream.CanRead) return;

            byte[] data = TryGetBlock();

            if (data.Length == 0)
                return;

            if (!HeaderCheck(data))
                return;

            SentToClients(data);
        }

        private bool HeaderCheck(byte[] data)
        {
            if (HeaderState == 2) return true;

            if(HeaderState == 0)
                initalHeader = data;

            if (HeaderState == 1)
                initalHeader = AppendData(initalHeader, data);

            HeaderState++;
            //Debugging.DebugLog(Debugging.DebugLevel.Debug1, "Header State: " + HeaderState);
            return false;
        }

        private byte[] TryGetBlock()
        {
            byte[] data = new byte[8192];
            int count = requestStream.Read(data, 0, data.Length);
            byte[] workingBlock;
            int HeaderPos = GetOggSHeader(data);

            //Debugging.DebugLog(Debugging.DebugLevel.Debug1, "Read header at " + HeaderPos);


            // If we start out with a header we try and get the next one, then continue on from there.
            if (HeaderPos == 0)
            {
                HeaderPos = GetOggSHeader(data, HeaderPos + 1);

                //Debugging.DebugLog(Debugging.DebugLevel.Debug1, "Read header marker at 0, new header at " + HeaderPos);
            }

            // If there is no header then the current block is not finished.
            if (HeaderPos == -1)
            {
                // New Array = Old Array + Current Array
                currentBlock = AppendData(currentBlock, data, count);

                //Debugging.DebugLog(Debugging.DebugLevel.Debug1, "Did not get new header. Current data length is " + currentBlock.Length);

                return new byte[0];
            }

            // Append all of the data up to the next OggS to the previously saved data and send it out.

            workingBlock = AppendData(currentBlock, data, HeaderPos);

            // Save all of the rest of the unsed data.
            currentBlock = new byte[count - HeaderPos];
            Array.Copy(data, HeaderPos, currentBlock, 0, count - HeaderPos);

            //Debugging.DebugLog(Debugging.DebugLevel.Debug1, workingBlock.Length + " bytes of data retrieved,  " + (count - HeaderPos) + " bytes saved");
            Console.WriteLine("========" + (workingBlock.Length + HeaderPos) + "================================================");
            //Console.WriteLine(BitConverter.ToString(workingBlock).Replace("-", ""));
            Console.WriteLine(tmp_dddREMOVEME(ASCIIEncoding.ASCII.GetString(workingBlock)));

            // return the previously clipped data
            return workingBlock;
        }

        private string tmp_dddREMOVEME(string s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (32 <= c && c <= 126)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.AppendFormat(".");
                }
            }
            return sb.ToString();
        }

        private void SentToClients(byte[] data)
        {
            List<IPCamStreamClient> toRemove = new List<IPCamStreamClient>();

            foreach (IPCamStreamClient client in clients)
            {
                if (!client.SentHeader)
                {
                    try
                    {
                        client.ClientStream.Write(initalHeader, 0, initalHeader.Length);
                    }
                    catch { toRemove.Add(client); }
                    client.Sent();
                }

                try
                {
                    client.ClientStream.Write(data, 0, data.Length);
                }
                catch { toRemove.Add(client); }
            }
            foreach (IPCamStreamClient client in toRemove)
                clients.Remove(client);
        }

        private byte[] AppendData(byte[] a, byte[] b)
        {
            return AppendData(a, b, b.Length);
        }

        private byte[] AppendData(byte[] a, byte[] b, int b_length)
        {
            byte[] newData = new byte[a.Length + b_length];
            Array.Copy(a, 0, newData, 0, a.Length);
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
                if (data[i] == 'O' && data[i + 1] == 'g' &&  data[i + 2] == 'g' && data[i + 3] == 'S')
                    return i;
                // OggS
            }
            return -1;
        }

        public void Start()
        {
            StopClients();
            clients = new List<IPCamStreamClient>();
            HttpWebRequest request = HttpWebRequest.CreateHttp("http://10.0.0.100/cameras/cam1.ogg");
            WebResponse = request.GetResponse();
            requestStream = WebResponse.GetResponseStream();

            currentBlock = new byte[0];
        }

        public void Stop()
        {
            StopClients();
            
            try { requestStream.Close(); } catch { }
            try { WebResponse.Close(); } catch { }
        }

        private void StopClients()
        {
            if (clients != null)
                foreach (IPCamStreamClient client in clients)
                    try { client.ClientStream.Close(); } catch { }
        }
    }
}
