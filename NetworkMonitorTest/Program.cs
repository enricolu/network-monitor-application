using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NdisMonitor;
using NetworkMonitor;

namespace NetworkMonitorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            NetworkMonitor.NetworkMonitor monitor = new NetworkMonitor.NetworkMonitor();
            monitor.StartListening();
        }

        static void monitor_PacketReceived(object sender, Packet packet)
        {
            Packet p = packet;
            Console.WriteLine("From {0} to {1}", p.IpHeader.SourceHostName, p.IpHeader.DestinationHostName);
            Console.WriteLine("Protocol: {0}", p.IpHeader.Protocol);
        }
    }
}
