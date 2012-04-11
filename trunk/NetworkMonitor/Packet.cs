using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;
using NtTdiApiWrapper;

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
        public Packet(byte[] data, PacketDirection direction) : base(data)
        {
            this.PacketDirection = direction;

            ArrivalTime = DateTime.Now;

            EthernetHeader ethernetHeader = new EthernetHeader(data); 
            IpHeader ipHeader = new IpHeader(this, ethernetHeader);

            this.SourceIpAddress = ipHeader.SourceIpAddress;
            this.DestinationIpAddress = ipHeader.DestinationIpAddress;
            this.Protocol = ipHeader.Protocol;

            this.Data = null;
        }

        public Packet(LOG_INFO logInfo)
        {
            this.ArrivalTime = DateTime.Now;
            this.Protocol = (IpProtocol) logInfo.m_Protocol;
            this.ProcessName = logInfo.m_szProcessName;

            if (logInfo.m_EvtType == TDIFLT_EVENT_TYPE.TDI_EVT_SND
                || logInfo.m_EvtType == TDIFLT_EVENT_TYPE.TDI_EVT_SND_DGM)
            {
                this.PacketDirection = PacketDirection.Uploading;
            }
            else
            {
                this.PacketDirection = PacketDirection.Downloading;
            }

            if(this.PacketDirection == PacketDirection.Downloading)
            {
                this.SourceIpAddress = GetIpAddress(logInfo.m_RemoteAddress.m_Ip, logInfo.m_RemoteAddress.m_Port);
                this.DestinationIpAddress = GetIpAddress(logInfo.m_LocalAddress.m_Ip, logInfo.m_LocalAddress.m_Port);
            }
            else
            {
                this.SourceIpAddress = GetIpAddress(logInfo.m_LocalAddress.m_Ip, logInfo.m_LocalAddress.m_Port);
                this.DestinationIpAddress = GetIpAddress(logInfo.m_RemoteAddress.m_Ip, logInfo.m_RemoteAddress.m_Port);
            }

            this.Size = (int)logInfo.m_DataLength;
        }

        private string GetIpAddress(uint address,ushort port)
        {
            IPEndPoint endPoint = new IPEndPoint(Convert.ToInt64(address),
                (int)IPAddress.NetworkToHostOrder((long)port));
            return endPoint.ToString();
        }

        internal static void CompressPacket(Packet packet, out MemoryStream outStream)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, packet);

            byte[] input = memoryStream.ToArray();
            byte[] output = new byte[input.Length + 1];
            LZF compressor = new LZF();
            int outLength = compressor.Compress(input, input.Length, output, output.Length);
            output = output.Take(outLength).ToArray();
            outStream = new MemoryStream(output);

            memoryStream.Close();
            outStream.Position = 0;
        }

        internal static void DecompressPacket(byte[] compressedData, out MemoryStream outStream)
        {
            byte[] decompressed = new byte[1000];
            LZF decompressor = new LZF();
            int decompressedLength = decompressor.Decompress(compressedData, compressedData.Length, decompressed, decompressed.Length);
            decompressed = decompressed.Take(decompressedLength).ToArray();

            outStream = new MemoryStream(decompressed);
            outStream.Position = 0;
        }

        /// <summary>
        /// Gets the remote host name
        /// </summary>
        public String HostName
        {
            get
            {
                try
                {
                    string ip = this.DestinationIpAddress.Split(':')[0];

                    if(PacketDirection == PacketDirection.Downloading)
                    {
                        ip = this.SourceIpAddress.Split(':')[0];
                    }

                    return Dns.GetHostEntry(ip).HostName;
                }
                catch (Exception ex)
                {
                    return "Unknown";
                }
            }
        }

        public PacketDirection PacketDirection { get; private set; }
        public DateTime ArrivalTime { get; private set; }
        public string SourceIpAddress { get; private set; }
        public string DestinationIpAddress { get; private set; }
        public string ProcessName { get; private set; }
        public IpProtocol Protocol { get; private set; }
    }
}
