using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Timers;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using NdisMonitor;
using NtTdiApiWrapper;
using Timer = System.Timers.Timer;

namespace NetworkMonitor
{
    public delegate void PacketReceviedEventHandler(Packet p);

    /// <summary>
    /// The class that controls traffic monitoring
    /// </summary>
    public class NetworkMonitor : INotifyPropertyChanged
    {
        private IntPtr buffer;
        private uint bufferSize = 0x10000;
        private int driver = 0;
        private bool listening = true;
        private const int PACKET_BUFFER = 25;
        private const string PACKET_DATABASE = "packets.bin";
        private ulong totalPackets;
        private decimal totalDownloaded;
        private decimal totalUploaded; 

        public event PropertyChangedEventHandler PropertyChanged;
        public event PacketReceviedEventHandler PacketReceived;

        public NetworkMonitor()
        {
            totalPackets = 0;
            totalDownloaded = 0;
            totalUploaded = 0;
            this.Filter = new PacketFilter();
        }

        protected void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        protected virtual void OnPacketReceived(Packet p)
        {
            if (PacketReceived != null)
                PacketReceived(p);
        }

        protected void InitializeDriver()
        {
            FILTER_INFO filterInfo;
            driver = NtTdiApi.OpenFilterDriverW(NtTdiApi.NTTDIDR_DRIVER_NAMEW);
            if (driver == 0)
            {
                throw new Exception("Cannot open driver");
            }

            uint driverVersion = NtTdiApi.GetDriverVersion(driver);
            if (driverVersion != NtTdiApi.NTTDIDR_DRIVER_VERSION)
            {
                throw new Exception("Unsupported driver version");
            }

            buffer = Marshal.AllocHGlobal(new IntPtr(bufferSize));
            if (buffer == IntPtr.Zero)
            {
                throw new Exception(string.Format("Cannot allocate {0} bytes", bufferSize));
            }

            NtTdiApi.SetLoggingState(driver, 1);
            //NtTdiApi.SetMaximumLogSize(driver, 100); //limits the packet count to 100

            filterInfo = new FILTER_INFO(1,
                NtTdiApi.EVENT_MASK_FULL,
                NtTdiApi.FLT_ACTION_LOG,
                0,
                0x00000000,
                0x00000000,
                0,
                0xFFFF,
                PROTOCOL.PROTOCOL_ANY,
                0,
                0xFFF);

            NtTdiApi.RemoveAllFilters(driver);

            if (!NtTdiApi.AddStaticFilter(driver, filterInfo))
            {
                throw new Exception("Cannot set filter");
            }
        }

        /// <summary>
        /// Starts capturing the packets
        /// </summary>
        public void StartListening()
        {
            InitializeDriver();

            double downloaded = 0;
            double uploaded = 0;

            //get speed information every second
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

            int i = 0, j = 0, error = 0;
            uint bytesProcessed = 0;
            LOG_INFO logInfo;
            string protocol;

            while (listening)
            {
                uint bytesRead = bufferSize;

                if (NtTdiApi.ReadLogEx(driver, buffer, ref bytesRead))
                {
                    
                    i++;
                    bytesProcessed = 0;

                    for (j = 0; bytesProcessed < bytesRead; j++)
                    {
                        IntPtr currentPtr = new IntPtr(buffer.ToInt32() + bytesProcessed);
                        logInfo = (LOG_INFO)Marshal.PtrToStructure(currentPtr, typeof(LOG_INFO));

                        if(logInfo.m_LocalAddress.m_Ip != 0 && logInfo.m_RemoteAddress.m_Ip != 0)
                        {
                            Packet p = new Packet(logInfo);
                            OnPacketReceived(p);
                            Packet.SerializePacket(p, PACKET_DATABASE);

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
                                uploaded += (ulong)p.Size;
                                NotifyPropertyChanged("TotalUploaded");
                            }
                        }

                        bytesProcessed += Convert.ToUInt32(Marshal.SizeOf(typeof(LOG_INFO))) + logInfo.m_DataLength;
                    }
                }
                else
                {
                    error = Marshal.GetLastWin32Error();

                    if (error == 122/*ERROR_INSUFFICIENT_BUFFER*/)
                    {
                        bufferSize += 0x1000;
                        buffer = Marshal.ReAllocHGlobal(buffer, new IntPtr(bufferSize));

                        if (buffer == IntPtr.Zero)
                        {
                            throw new Exception(string.Format("Cannot allocate {0} bytes! abort", bufferSize));
                        }
                    }

                    if (NtTdiApi.GetWaitEvent(driver))
                    {
                        NtTdiApi.WaitForData(driver, 0xFFFFFFFF); //INFINITE
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
                
            }
        }

        /// <summary>
        /// Stops the traffic monitoring
        /// </summary>
        public void PauseListening()
        {
            listening = false;
            Marshal.FreeHGlobal(buffer);

            NtTdiApi.SetLoggingState(driver, 0);
            NtTdiApi.PurgeLog(driver);
            NtTdiApi.RemoveAllFilters(driver);

            this.DownloadSpeed = 0;
            this.UploadSpeed = 0;
        }

        public List<Packet> DeserializeAllPackets(string fileName = PACKET_DATABASE)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open);
            List<Packet> packets = new List<Packet>();

            BinaryReader binaryReader = new BinaryReader(fileStream);
            while (fileStream.Position <= fileStream.Length - 1)
            {
                int packetSize = binaryReader.ReadInt32();

                if (packetSize != 0)
                {
                    byte[] packetData = binaryReader.ReadBytes(packetSize);
                    MemoryStream packetStream = new MemoryStream();
                    Packet.DecompressPacket(packetData, packetStream);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    Packet p = (Packet)binaryFormatter.Deserialize(packetStream);
                    packets.Add(p);
                }
            }

            return packets;
        }

        /// <summary>
        /// Checks if the packet matches a specified filter
        /// </summary>
        /// <param name="p">the filterign options</param>
        /// <returns>true if the packet matches the filter</returns>
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
                && this.Filter.Protocol != p.Protocol)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Filters the packets
        /// </summary>
        public List<Packet> FilterPackets()
        {
            List<Packet> allPackets = this.DeserializeAllPackets();
            List<Packet> filtered = (from p in allPackets
                                     where PacketMatchesFilter(p)
                                     select p).ToList();
            return filtered;
        }
		

        public ulong TotalPackets
        {
            get { return this.totalPackets; }
        }

        /// <summary>
        /// The downloaded traffic in Megabytes
        /// </summary>
        public decimal TotalDownloaded
        {
            get { return Math.Round(this.totalDownloaded / 1000000, 3); }
        }

        /// <summary>
        /// The uploaded traffic in Megabytes
        /// </summary>
        public decimal TotalUploaded
        {
            get { return Math.Round(this.totalUploaded/1000000, 3); }
        }

        public double DownloadSpeed { get; private set; }
        public double UploadSpeed { get; set; }

        public PacketFilter Filter { get; private set; }
    }
}
