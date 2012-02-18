using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkMonitor
{
    public class TrafficStatistics : IDisposable
    {
        private List<Packet> packets;

        public TrafficStatistics(List<Packet> packets)
        {
            this.packets = packets;
        }

        public Dictionary<byte, decimal> GetDownloadedLastHour()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 60; i ++ )
            {
                foreach (var packet in this.packets)
                {
                    if(packet.PacketDirection == PacketDirection.Downloading
                        && packet.ArrivalTime.Minute == i
                        && (DateTime.Now - packet.ArrivalTime).Duration().Minutes <= 60 )
                    {
                        if(result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }    
            }

            return result;
        }

        public Dictionary<byte, decimal> GetUploadedLastHour()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 60; i++)
            {
                foreach (var packet in this.packets)
                {
                    if (packet.PacketDirection == PacketDirection.Uploading
                        && packet.ArrivalTime.Minute == i
                        && (DateTime.Now - packet.ArrivalTime).Duration().Minutes <= 60 )
                    {
                        if (result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<byte, decimal> GetDownloadedLastDay()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 24; i++)
            {
                foreach (var packet in this.packets)
                {
                    if (packet.PacketDirection == PacketDirection.Downloading
                        && packet.ArrivalTime.Hour == i
                        && (DateTime.Now - packet.ArrivalTime).Hours <= 24)
                    {
                        if (result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<byte, decimal> GetUploadedLastDay()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 24; i++)
            {
                foreach (var packet in this.packets)
                {
                    if (packet.PacketDirection == PacketDirection.Uploading
                        && packet.ArrivalTime.Hour == i
                        && (DateTime.Now - packet.ArrivalTime).Hours <= 24)
                    {
                        if (result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<byte, decimal> GetDownloadedLastWeek()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 24; i++)
            {
                foreach (var packet in this.packets)
                {
                    if (packet.PacketDirection == PacketDirection.Downloading
                        && packet.ArrivalTime.Hour == i
                        && (DateTime.Now - packet.ArrivalTime).Days <= 7)
                    {
                        if (result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<byte, decimal> GetUploadedLastWeek()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 24; i++)
            {
                foreach (var packet in this.packets)
                {
                    if (packet.PacketDirection == PacketDirection.Uploading
                        && packet.ArrivalTime.Hour == i
                        && (DateTime.Now - packet.ArrivalTime).Days <= 7)
                    {
                        if (result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<byte, decimal> GetDownloadedLastMonth()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 24; i++)
            {
                foreach (var packet in this.packets)
                {
                    if (packet.PacketDirection == PacketDirection.Downloading
                        && packet.ArrivalTime.Hour == i
                        && (DateTime.Now - packet.ArrivalTime).Days <= 31)
                    {
                        if (result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }
            }

            return result;
        }

        public Dictionary<byte, decimal> GetUploadedLastMonth()
        {
            Dictionary<byte, decimal> result = new Dictionary<byte, decimal>();

            for (byte i = 0; i < 24; i++)
            {
                foreach (var packet in this.packets)
                {
                    if (packet.PacketDirection == PacketDirection.Uploading
                        && packet.ArrivalTime.Hour == i
                        && (DateTime.Now - packet.ArrivalTime).Days <= 31)
                    {
                        if (result.ContainsKey(i))
                        {
                            result[i] += packet.Size;
                        }
                        else
                        {
                            result.Add(i, packet.Size);
                        }
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            this.packets.Clear();
            this.packets = null;
        }
    }
}
