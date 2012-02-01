using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;

namespace NetworkMonitor
{
    [Serializable]
    public enum PacketDirection
    {
        Downloading = 0,
        Uploading = 1
    }

    [Serializable]
    public class Packet : RawPacket
    {
        private EthernetHeader ethernetHeader;
        private IpHeader ipHeader;

        public Packet(byte[] data, byte direction) : base(data)
        {
            ethernetHeader = new EthernetHeader(data);
            ipHeader = new IpHeader(this, ethernetHeader);

            if(direction == 0)
            {
                PacketDirection = PacketDirection.Downloading;
            }
            else
            {
                PacketDirection = PacketDirection.Uploading;
            }

            ArrivalTime = DateTime.Now;
        }

        public static void Serialize(Packet p, string fileName)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, p);
            string base64Str = System.Convert.ToBase64String(memoryStream.ToArray());

            using(StreamWriter writer = new StreamWriter(fileName, true))
            {
                writer.WriteLine(base64Str);
            }
        }

        public EthernetHeader EthernetHeader
        {
            get { return this.ethernetHeader; }
        }
        public IpHeader IpHeader
        {
            get { return this.ipHeader; }
        }

        public String HostName
        {
            get
            {
                if (PacketDirection == PacketDirection.Downloading)
                {
                    return this.IpHeader.SourceHostName;
                }
                else
                {
                    return this.IpHeader.DestinationHostName;
                }
            }
        }

        public PacketDirection PacketDirection { get; private set; }
        public DateTime ArrivalTime { get; private set; }
    }
}
