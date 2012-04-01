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

            this.SourceMacAddress = ethernetHeader.SourceMacAddress;
            this.DestinationMacAddress = ethernetHeader.DestinationMacAddress;
            this.SourceIpAddress = ipHeader.SourceIpAddress;
            this.DestinationIpAddress = ipHeader.DestinationIpAddress;
            this.Protocol = ipHeader.Protocol;

            if (PacketDirection == PacketDirection.Downloading)
            {
                this.HostName = ipHeader.SourceHostName;
            }
            else
            {
                this.HostName = ipHeader.DestinationHostName;
            }

            this.Data = null;
        }

        public Packet(LOG_INFO logInfo)
        {
            this.ArrivalTime = DateTime.Now;
            this.Protocol = (IpProtocol) logInfo.m_Protocol;
            this.ProcessName = logInfo.m_szProcessName;

            if (logInfo.m_EvtType == TDIFLT_EVENT_TYPE.TDI_EVT_RCV
                || logInfo.m_EvtType == TDIFLT_EVENT_TYPE.TDI_EVT_RCV_DGM)
            {
                this.PacketDirection = PacketDirection.Downloading;
            }
            else if (logInfo.m_EvtType == TDIFLT_EVENT_TYPE.TDI_EVT_SND
                || logInfo.m_EvtType == TDIFLT_EVENT_TYPE.TDI_EVT_SND_DGM)
            {
                this.PacketDirection = PacketDirection.Uploading;
            }
            else
            {
                return;
            }

            if(this.PacketDirection == PacketDirection.Downloading)
            {
                this.SourceIpAddress = GetIpAddress(logInfo.m_RemoteAddress.m_Ip, logInfo.m_RemoteAddress.m_Port);
                this.DestinationIpAddress = GetIpAddress(logInfo.m_LocalAddress.m_Ip, logInfo.m_LocalAddress.m_Port);

                try
                {
                    this.HostName = Dns.GetHostEntry(this.SourceIpAddress).HostName;
                }
                catch (Exception ex)
                {
                    this.HostName = IpHeader.UNKNOWN_HOST;
                }
            }
            else
            {
                this.SourceIpAddress = GetIpAddress(logInfo.m_LocalAddress.m_Ip, logInfo.m_LocalAddress.m_Port);
                this.DestinationIpAddress = GetIpAddress(logInfo.m_RemoteAddress.m_Ip, logInfo.m_RemoteAddress.m_Port);

                try
                {
                    this.HostName = Dns.GetHostEntry(this.DestinationIpAddress).HostName;
                }
                catch (Exception ex)
                {
                    this.HostName = IpHeader.UNKNOWN_HOST;
                }
            }

            this.SourceMacAddress = "nnn";
            this.DestinationMacAddress = "nnn";
            this.Size = (int)logInfo.m_DataLength;
        }

        private string GetIpAddress(uint address,ushort port)
        {
            IPEndPoint endPoint = new IPEndPoint(Convert.ToInt64(address),
                IPAddress.NetworkToHostOrder((short)port));
            return endPoint.ToString();
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
        /// Gets the remote host name
        /// </summary>
        public String HostName { get; private set; }

        public PacketDirection PacketDirection { get; private set; }
        public DateTime ArrivalTime { get; private set; }
        public string SourceIpAddress { get; private set; }
        public string DestinationIpAddress { get; private set; }
        public string SourceMacAddress { get; private set; }
        public string DestinationMacAddress { get; private set; }
        public string ProcessName { get; private set; }
        public IpProtocol Protocol { get; private set; }
    }
}
