using CashLib.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace CashCam.Stream
{

    class IPCamStreamRepeater : IThreadTask
    {
        private List<System.IO.Stream> streams;
        WebResponse WebResponse;
        System.IO.Stream requestStream;
        byte[] initalHeader;
        bool ReadInHeader = false;
        bool ReadInHeader2 = false;
        byte[] leftovers;

        string hostname;
        private IPCamStreamTask iPCamStreamTask;

        public IPCamStreamRepeater(string hostname)
        {
            this.hostname = hostname;
          //  Program.CameraRepater = this;
        }

        public IPCamStreamRepeater(IPCamStreamTask iPCamStreamTask, string hostname)
        {
            this.iPCamStreamTask = iPCamStreamTask;
            this.hostname = hostname;
        }

        public void AddStream(System.IO.Stream stream)
        {
            if (!ReadInHeader2) throw new Exception("HEADER NOT READ IN YET< SORRY!");

            try { stream.Write(initalHeader, 0, initalHeader.Length); } catch { return; }
            streams.Add(stream);
        }

        public void RunTask()
        {
            if (WebResponse == null || requestStream == null) { Start(); return; };
            if (!requestStream.CanRead) return;


            byte[] data = new byte[8192];
            int count = requestStream.Read(data, 0, data.Length);
            byte[] newLeftOvers;
            int HeaderPos = getOggSHeader(data);
            if (HeaderPos == -1)
            {
                newLeftOvers = new byte[leftovers.Length + count];
                Array.Copy(leftovers, newLeftOvers, leftovers.Length);
                Array.Copy(data, 0, newLeftOvers, leftovers.Length, count);
                leftovers = newLeftOvers;
                return;
            }
            if (HeaderPos == 0)
            {
                int HeaderPos2 = getOggSHeader(data, HeaderPos);
                if (HeaderPos2 == -1) //We got a start but no end
                {
                    newLeftOvers = new byte[leftovers.Length + count];
                    Array.Copy(leftovers, newLeftOvers, leftovers.Length);
                    Array.Copy(data, 0, newLeftOvers, leftovers.Length, count);
                    leftovers = newLeftOvers;
                    return;
                }
                //We got a full block. We trick the logic which makes no sense.
                HeaderPos = HeaderPos2;
            }

            //Append all of the data up to the next OggS to leftovers then push out leftovers.


            newLeftOvers = new byte[leftovers.Length + HeaderPos];
            Array.Copy(leftovers, newLeftOvers, leftovers.Length);
            Array.Copy(data, 0, newLeftOvers, leftovers.Length, HeaderPos);

            //Push remaining data onto leftovers

            leftovers = new byte[count - HeaderPos];
            Array.Copy(data, HeaderPos, leftovers, 0, count - HeaderPos);

            //Console.WriteLine("========================================================");
            //Console.WriteLine(BitConverter.ToString(leftovers));

            List<System.IO.Stream> toRemove = new List<System.IO.Stream>();



            if (!ReadInHeader)
            {
                initalHeader = newLeftOvers;
                ReadInHeader = true;
                return;
            }
            if (!ReadInHeader2)
            {
                List<byte> nb = new List<byte>();
                nb.AddRange(initalHeader);
                nb.AddRange(newLeftOvers);
                initalHeader = nb.ToArray();
                ReadInHeader2 = true;
                return;
            }


            foreach (System.IO.Stream client in streams)
            {
                try
                {
                    client.Write(newLeftOvers, 0, newLeftOvers.Length);
                }
                catch { toRemove.Add(client); }
            }
            foreach (System.IO.Stream client in toRemove)
                streams.Remove(client);


        }

        private int getOggSHeader(byte[] data, int offset = 0, int limit = -1)
        {
            if (limit == -1) limit = data.Length;
            string s = ASCIIEncoding.ASCII.GetString(data, 0, limit);
            return s.IndexOf("OggS", offset);
            /*
            for (int i = offset; i < limit - 4; i++)
            {
                if (data[i] == 'O' && data[i + 1] == 'g' &&  data[i + 2] == 'g' && data[i + 3] == 's')
                    return i;
                //OggS
            }*/
            // return -1;
        }

        public void Start()
        {
            if (streams != null)
                foreach (System.IO.Stream client in streams)
                    try { client.Close(); } catch { }
            streams = new List<System.IO.Stream>();
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(hostname);
                WebResponse = request.GetResponse();
                requestStream = WebResponse.GetResponseStream();
            }
            catch { WebResponse = null; }

            // byte[] data = new byte[8192];
            // int count = requestStream.Read(data, 0, data.Length);

            leftovers = new byte[0];

            //initalHeader = new byte[count];
            //Array.ConstrainedCopy(data, 0, initalHeader, 0, count);

            //Console.WriteLine(ASCIIEncoding.ASCII.GetString(data));
            //Console.WriteLine(count);
        }

        public void Stop()
        {
            foreach (System.IO.Stream client in streams)
                try { client.Close(); } catch { }

            try { requestStream.Close(); } catch { }
            try { WebResponse.Close(); } catch { }
        }
    }
}