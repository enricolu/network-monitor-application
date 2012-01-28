﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class NetworkMonitor : INotifyPropertyChanged
    {
        private NdisHookStubs.NT_PROTOCOL_LIST protocolList;
        private List<Adapter> adapters;
		private List<Packet> packets = new List<Packet>();
        private const int PACKET_BUFFER = 50;
        private ulong totalPackets;
        private decimal totalDownloaded;
        private decimal totalUploaded;

        public event PacketReceivedEventHandler PacketReceived;
        public event PropertyChangedEventHandler PropertyChanged;

        public NetworkMonitor()
        {   
            InitializeProtocolList();
            this.adapters = GetAdapters();
            totalPackets = 0;
            totalDownloaded = 0;
            totalUploaded = 0;
            this.PacketsToDisplay = new List<Packet>();
        }

        protected void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public void StartListening(List<Adapter> adaptersToListen = null)
        {
            if(adaptersToListen == null)
            {
                adaptersToListen = this.adapters;
            }

            protocolList.Listen((from a in adaptersToListen select a.AdapterID).ToArray());

            Thread t = new Thread(() =>
                {
                    while (true)
                    {
                        NdisHookStubs.NEXT_PACKET nextPacket = NdisHookStubs.NEXT_PACKET.WaitFor();
                        if (nextPacket != null)
                        {
                            Packet p = new Packet(nextPacket._data, nextPacket._bDirection);
                            PacketReceived(this, p);
                            packets.Add(p);
                            NotifyPropertyChanged("Packets");

                            totalPackets++;
                            NotifyPropertyChanged("TotalPackets");

                            if (p.PacketDirection == PacketDirection.Downloading)
                            {
                                totalDownloaded += p.Size;
                                NotifyPropertyChanged("TotalDownloaded");
                            }
                            else if (p.PacketDirection == PacketDirection.Uploading)
                            {
                                totalUploaded += p.Size;
                                NotifyPropertyChanged("TotalUploaded");
                            }

                            //if (this.packets.Count >= PACKET_BUFFER * 2)
                            //{
                            //    SerializePackets(PACKET_BUFFER);
                            //}
                        }
                    }
                });

            t.IsBackground = true;
            t.Start();
        }

        public void PauseListening()
        {
            protocolList.Stop();
            this.SerializePackets(this.Packets.Count);
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
            try
            {
                using(Stream stream = File.Open("packets.bin", FileMode.OpenOrCreate))
                {
                    BinaryFormatter binFormatter = new BinaryFormatter();
                    List<Packet> packetsToSerialize = this.packets.Take(packetCount).ToList();
                    binFormatter.Serialize(stream, packetsToSerialize);
                    //this.packets.RemoveRange(0, packetCount);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException("Cannon serialize data", ex);
            }
        }
		
		public List<Packet> Packets
		{
			get {return this.packets; }
		}

        public List<Packet> PacketsToDisplay { get; private set; }

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