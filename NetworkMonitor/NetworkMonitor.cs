using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NdisMonitor;

namespace NetworkMonitor
{
    public delegate void PacketReceivedEventHandler(object sender, Packet packet);

    public class NetworkMonitor
    {
        private NdisHookStubs.NT_PROTOCOL_LIST protocolList;
        private List<Adapter> adapters;
		private List<Packet> packets = new List<Packet>();
        private const int MAXIMUM_PACKETS = 50;
        private ulong totalPackets;
        private decimal totalDownloaded;
        private decimal totalUploaded;

        public event PacketReceivedEventHandler PacketReceived;

        public NetworkMonitor()
        {   
            InitializeProtocolList();
            this.adapters = GetAdapters();
            totalPackets = 0;
            totalDownloaded = 0;
            totalUploaded = 0;
        }

        public void StartListening(List<Adapter> adaptersToListen = null)
        {
            if(adaptersToListen == null)
            {
                adaptersToListen = this.adapters;
            }

            protocolList.Listen((from a in adaptersToListen select a.AdapterID).ToArray());

            while (true)
            {
                NdisHookStubs.NEXT_PACKET nextPacket = NdisHookStubs.NEXT_PACKET.WaitFor();
                if (nextPacket != null)
                {
                    Packet p = new Packet(nextPacket._data, nextPacket._bDirection);
                    PacketReceived(this, p);
                    packets.Add(p);
                    totalPackets++;

                    if(p.PacketDirection == PacketDirection.Downloading)
                    {
                        totalDownloaded += p.Size;
                    }
                    else if(p.PacketDirection == PacketDirection.Uploading)
                    {
                        totalUploaded += p.Size;
                    }
                    
                    SerializePackets(MAXIMUM_PACKETS);
                    Thread.Sleep(10);
                }
            }
        }

        public List<Adapter> GetAdapters()
        {
            if(protocolList == null)
            {
                InitializeProtocolList();
            }

            List<Adapter> adapters = new List<Adapter>();

            foreach (NdisHookStubs.NT_OPEN_ADAPTER adapter in protocolList._adapters)
            {
                adapters.Add(new Adapter(adapter.adapterName, adapter._dwOrdinal));
            }

            return adapters;
        }

        private void InitializeProtocolList()
        {
            while (true)
            {
                protocolList = NdisHookStubs.NT_PROTOCOL_LIST.Start(32, 0, 0);

                if (protocolList == null)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    break;
                }
            }
        }

        private void SerializePackets(int packetCount)
        {
            if(this.packets.Count >= packetCount + 1)
            {
                try
                {
                    using(Stream stream = File.Open("packets.bin", FileMode.OpenOrCreate))
                    {
                        BinaryFormatter binFormatter = new BinaryFormatter();
                        List<Packet> packetsToSerialize = this.packets.Take(packetCount).ToList();
                        if(packetsToSerialize != null)
                        {
                            binFormatter.Serialize(stream, packetsToSerialize);
                            packets.RemoveRange(0, packetCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new SerializationException("Cannon serialize data", ex);
                }
            }
        }
		
		public List<Packet> Packets
		{
			get {return packets; }
		}

        public ulong TotalPackets
        {
            get { return this.totalPackets; }
        }

        public decimal TotalDownloaded
        {
            get { return this.totalDownloaded; }
        }

        public decimal TotalUploaded
        {
            get { return this.totalUploaded; }
        }
    }
}
