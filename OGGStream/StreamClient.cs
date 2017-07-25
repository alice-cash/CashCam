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
        public System.IO.Stream Stream;
        private Func<byte[], int, int, bool> writeFunction;
        public bool SentHeader;
        public int ID { get; private set; }

        private bool OffsetSet;
        private ulong Offset;

        private static Random r = new Random();


        public StreamClient(System.IO.Stream stream, Func<byte[],int,int,bool> action)
        {
            writeFunction = action;
            Stream = stream;
            SentHeader = false;
            ID = r.Next();
            OffsetSet = false;
        }

        public bool Write(byte[] data, int offset, int length)
        {

            //We need to offset the page sequence otherwise the recipient gets a video starting at some unknown time

            ulong PacketPosition = BitConverter.ToUInt64(data, 6);
            ulong PageSequence = BitConverter.ToUInt64(data, 18);
            if (!OffsetSet && PageSequence >= 2)
            {
                Offset = PacketPosition;
                OffsetSet = true;
            }

            if(OffsetSet)
            {
                byte[] newPosition =  BitConverter.GetBytes(PacketPosition - Offset);
                for (int i = 0; i < 8; i++)
                    data[i + 6] = newPosition[i];
            }

            return writeFunction(data, offset, length);
        }
    }
}
