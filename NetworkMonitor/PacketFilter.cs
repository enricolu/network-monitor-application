using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkMonitor
{
    /// <summary>
    /// Contains options for filtering the packets
    /// </summary>
    [Serializable]
    public class PacketFilter
    {
        public PacketFilter(IpProtocol? protocol = null, PacketDirection? direction = null, string host = null)
        {
            this.Protocol = protocol;
            this.Direction = direction;
            this.Host = host;
        }

        public IpProtocol? Protocol { get; set; }
        public PacketDirection? Direction { get; set; }
        public string Host { get; set; }
        public bool IsEmpty
        {
            get
            {
                return (this.Protocol == null
                        && this.Direction == null
                        && this.Host == null);
            }
        }
    }
}
