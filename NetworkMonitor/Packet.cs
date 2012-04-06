using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.IO;
using NtTdiApiWrapper;
using SevenZip;

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

        private static void CompressPacket(Packet packet, MemoryStream outStream)
        {
            SevenZipCompressor.SetLibraryPath(@"7z.dll");
            SevenZip.SevenZipCompressor compressor = new SevenZipCompressor();
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memoryStream, packet);
            compressor.CompressStream(memoryStream, outStream);

            memoryStream.Close();
            outStream.Position = 0;
        }

        internal static void DecompressPacket(byte[] compressedData, Stream outStream)
        {
            SevenZipCompressor.SetLibraryPath(@"7z.dll");
            MemoryStream inStream = new MemoryStream(compressedData);
            SevenZipExtractor extractor = new SevenZipExtractor(inStream);
            extractor.ExtractFile(0, outStream);
            outStream.Position = 0;
        }

        public static void SerializePacket(Packet packet, string fileName)
        {
            using (FileStream fileStream = new FileStream(fileName, FileMode.Append))
            {
                MemoryStream compressedPacketStream = new MemoryStream();
                CompressPacket(packet, compressedPacketStream);

                BinaryWriter writer = new BinaryWriter(fileStream);
                writer.Write(compressedPacketStream.Capacity);
                compressedPacketStream.WriteTo(fileStream);
                long zipSignatureLength = compressedPacketStream.Capacity - compressedPacketStream.Length;
                byte[] zipSignature = new byte[zipSignatureLength];
                writer.Write(zipSignature);

                compressedPacketStream.Close();
                writer.Close();
            }
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
