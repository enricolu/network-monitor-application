﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Timers;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NdisMonitor;
using Timer = System.Timers.Timer;

namespace NetworkMonitor
{
    public class NetworkMonitor : INotifyPropertyChanged
    {
        private NdisHookStubs.NT_PROTOCOL_LIST protocolList;
        private List<Adapter> adapters;
		private List<Packet> packets = new List<Packet>();
        private const int PACKET_BUFFER = 25;
        private const string PACKET_DATABASE = "packets.bin";
        private ulong totalPackets;
        private decimal totalDownloaded;
        private decimal totalUploaded; 

        public event PropertyChangedEventHandler PropertyChanged;

        public NetworkMonitor()
        {   
            InitializeProtocolList();
            this.adapters = GetAdapters();
            totalPackets = 0;
            totalDownloaded = 0;
            totalUploaded = 0;
            this.Filter = new PacketFilter();
            this.TrafficStatistics = new TrafficStatistics(this.packets);
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
            if (adaptersToListen == null)
            {
                adaptersToListen = this.adapters;
            }

            protocolList.Listen((from a in adaptersToListen select a.AdapterID).ToArray());

            double downloaded = 0;
            double uploaded = 0;

            Timer t = new Timer(1000);
            t.Elapsed += new ElapsedEventHandler((e, o) =>
            {
                if (downloaded != 0)
                {
                    this.DownloadSpeed = downloaded / 1000;
                    NotifyPropertyChanged("DownloadSpeed");
                    downloaded = 0;
                }

                if (uploaded != 0)
                {
                    this.UploadSpeed = uploaded / 1000;
                    NotifyPropertyChanged("UploadSpeed");
                    uploaded = 0;
                }
            });
            t.Start();

            while (true)
            {
                

                NdisHookStubs.NEXT_PACKET nextPacket =
                    NdisHookStubs.NEXT_PACKET.WaitFor();

                if (nextPacket != null)
                {
                    Packet p = new Packet(nextPacket._data,
                                            nextPacket._bDirection);

                    packets.Add(p);
                    NotifyPropertyChanged("Packets");

                    totalPackets++;
                    NotifyPropertyChanged("TotalPackets");

                    if (p.PacketDirection == PacketDirection.Downloading)
                    {
                        totalDownloaded += p.Size;
                        downloaded += (ulong)p.Size;
                        NotifyPropertyChanged("TotalDownloaded");
                    }
                    else if (p.PacketDirection ==
                                PacketDirection.Uploading)
                    {
                        totalUploaded += p.Size;
                        uploaded += (ulong) p.Size;
                        NotifyPropertyChanged("TotalUploaded");
                    }

                    if (this.packets.Count >= PACKET_BUFFER * 2)
                    {
                        SerializePackets(PACKET_BUFFER);
                    }
                }
            }
        }

        public void PauseListening()
        {
            protocolList.Stop();
            this.SerializePackets(this.Packets.Count, false);
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

        private void SerializePackets(int packetCount, bool delete = true)
        {
            try
            {
                for (int i = 0; i < packetCount; i ++ )
                {
                    Packet.Serialize(Packets[i], PACKET_DATABASE);
                }

                if (delete)
                {
                    this.packets.RemoveRange(0, packetCount);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException("Cannon serialize data", ex);
            }
        }

        public List<Packet> DeserializeAllPackets()
        {
            List<Packet> packets = new List<Packet>();
            StreamReader streamReader = new StreamReader(PACKET_DATABASE);

            while(!streamReader.EndOfStream)
            {
                string base64Str = streamReader.ReadLine();
                byte[] packetData = Convert.FromBase64String(base64Str);
                MemoryStream memoryStream = new MemoryStream(packetData);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                packets.Add((Packet)binaryFormatter.Deserialize(memoryStream));
            }

            return packets;
        }

        public bool PacketMatchesFilter(Packet p)
        {
            if(this.Filter.Direction != null 
                && this.Filter.Direction != p.PacketDirection)
            {
                return false;
            }

            if(this.Filter.Host != null
                && !p.HostName.Contains(this.Filter.Host))
            {
                return false;
            }

            if(this.Filter.Protocol != null
                && this.Filter.Protocol != p.IpHeader.Protocol)
            {
                return false;
            }

            return true;
        }

        public void FilterPackets()
        {
            List<Packet> allPackets = this.DeserializeAllPackets();
            List<Packet> filtered = (from p in allPackets
                                     where PacketMatchesFilter(p)
                                     select p).ToList();
            this.Packets = filtered;
        }
		
		public List<Packet> Packets
		{
			get {return this.packets; }
            private set 
            { 
                this.packets = value;
                NotifyPropertyChanged("Packets");
            }
		}

        public ulong TotalPackets
        {
            get { return this.totalPackets; }
        }

        public decimal TotalDownloaded
        {
            get { return Math.Round(this.totalDownloaded / 1000000, 3); }
        }

        public decimal TotalUploaded
        {
            get { return Math.Round(this.totalUploaded/1000000, 3); }
        }

        public double DownloadSpeed { get; private set; }
        public double UploadSpeed { get; set; }

        public PacketFilter Filter { get; private set; }
        public TrafficStatistics TrafficStatistics { get; private set; }
    }
}
