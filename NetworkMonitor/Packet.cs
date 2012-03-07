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

    /// <summary>
    /// Represents an IP packet
    /// </summary>
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
        
        /// <summary>
        /// Serializes a packet into a file
        /// </summary>
        /// <param name="p">The packet to serialize</param>
        /// <param name="fileName">The packet database file</param>
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

        /// <summary>
        /// Contains lower-level information about the packet 
        /// </summary>
        public EthernetHeader EthernetHeader
        {
            get { return this.ethernetHeader; }
        }

        /// <summary>
        /// Contains higher-level information about the packet
        /// </summary>
        public IpHeader IpHeader
        {
            get { return this.ipHeader; }
        }

        /// <summary>
        /// Gets the remote host name
        /// </summary>
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
