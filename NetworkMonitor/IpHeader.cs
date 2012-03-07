using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace NetworkMonitor
{
    /// <summary>
    /// List of IP protocols
    /// </summary>
    [Serializable]
    public enum IpProtocol
    {
        HOPOPT = 0, // IPv6 Hop-by-Hop Option
        ICMP = 1, // Internet Control Message Protocol
        IGAP = 2, // IGMP for user Authentication Protocol
        GGP = 3, // Gateway to Gateway Protocol
        IP_in_IP_encap = 4, // IP in IP encapsulation
        IST = 5, // Internet Stream Protocol
        TCP = 6, // Transmission Control Protocol
        UCL = 7, // ???
        EGP = 8, // Exterior Gateway Protocol
        IGRP = 9, // Interior Gateway Routing Protocol
        BBN_RCC_Monit = 10, // BBN RCC Monitoring
        NVP = 11, // Network Voice Protocol
        PUP = 12, // ???
        ARGUS = 13, // ???
        EMCON = 14, // Emission Control Protocol
        XNET = 15, // Cross Net Debugger
        Chaos = 16, // ???
        UDP = 17, // User Datagram Protocol
        TMux = 18, // Transport Multiplexing Protocol
        DCN_Mea_Subs = 19, // DCN Measurement Subsystems
        HMP = 20, // Host Monitoring Protocol
        PRM = 21, // Packet Radio Measurement
        Xerox_NS_IDP = 22, // XEROX NS IDP
        Trunk_1 = 23, // ???
        Trunk_2 = 24, // ???
        Leaf_1 = 25, // ???
        Leaf_2 = 26, // ???
        RDP = 27, // Reliable Data Protocol
        IRTP = 28, // Internet Reliable Transaction Protocol
        ISO_TPC4 = 29, // ISO Transport Protocol Class 4
        NETBLT = 30, // Network Block Transfer
        MFE = 31, // MFE Network Services Protocol
        MERIT = 32, // MERIT Internodal Protocol
        SEP = 33, // Sequential Exchange Protocol
        TPCP = 34, // Third Party Connect Protocol
        IDPR = 35, // Inter-Domain Policy Routing Protocol
        XTP = 36, // Xpress Transfer Protocol
        DDP = 37, // Datagram Delivery Protocol
        CMTP = 38, // Control Message Transport Protocol
        TPPP = 39, // TP++ Transport Protocol
        IL = 40, // IL Transport Protocol
        IPv6_over_IPv4 = 41, // IPv6 over IPv4
        SDRP = 42, // Source Demand Routing Protocol
        IPv6RH = 43, // IPv6 Routing header
        IPv6FH = 44, // IPv6 Fragment header
        IDRP = 45, // Inter-Domain Routing Protocol
        RSVP = 46, // Reservation Protocol
        GRE = 47, // General Routing Encapsulation
        MHRP = 48, // Mobile Host Routing Protocol
        BNA = 49, // ???
        ESP = 50, // Encapsulating Security Payload
        AH = 51, // Authentication Header
        INLSTUBA = 52, // Integrated Net Layer Security TUBA
        IPEnc = 53, // IP with Encryption
        NARP = 54, // NBMA Address Resolution Protocol
        MEP = 55, // Minimal Encapsulation Protocol
        TLSP = 56, // Transport Layer Security Protocol using Kryptonet key management
        SKIP = 57, // ???
        ICMPv6 = 58, // Internet Control Message Protocol for IPv
        IPv6_No_Next_Head = 59, // IPv6 No Next Header
        IPv6DestOpt = 60, // Destination Options for IPv6
        AnyHost = 61, // Any host internal protocol
        CFTP = 62, // ???
        AnyLocal = 63, // Any local network
        EXPAK = 64, // SATNET and Backroom EXPAK
        Kryptolan = 65, // ???
        MIT_Remote_VDisk = 66, // MIT Remote Virtual Disk Protocol
        Internet_Pluribus = 67, // Internet Pluribus Packet Core
        AnyDistrFS = 68, // Any distributed file system
        SATNET_Monitor = 69, // SATNET Monitoring
        VISA = 70, // VISA Protocol
        Internet_Packet = 71, // Internet Packet Core Utility
        Net_Exe = 72, // Computer Protocol Network Executive
        Heart_Beat = 73, // Computer Protocol Heart Beat
        Wang_Span = 74, // Wang Span Network
        Packet_Video = 75, // Packet Video Protocol
        Backr_SATNET_Monitor = 76, // Backroom SATNET Monitoring
        SUN_ND_temp = 77, // SUN ND PROTOCOL-Temporary
        WIDEBAND_Monitor = 78, // WIDEBAND Monitoring
        WIDEBAND_EXPAK = 79, // WIDEBAND EXPAK
        ISO_IP = 80, // ISO-IP
        VMTP = 81, // Versatile Message Transaction Protocol
        SECURE_VMT = 82, // ???
        VINES = 83, // ???
        TTP = 84, // ???
        NSFNET_IGP = 85, // ???
        DGP = 86, // Dissimilar Gateway Protocol
        TCF = 87, // ???
        EIGRP = 88, // ???
        OSPF = 89, // Open Shortest Path First Routing Protocol
        SRPCP = 90, // Sprite RPC Protocol
        LARP = 91, // Locus Address Resolution Protocol
        MTP = 92, // Multicast Transport Protocol
        AX25 = 93, // AX.25
        IP_in_IP = 94, // IP-within-IP Encapsulation Protocol
        MICP = 95, // Mobile Internetworking Control Protocol
        Semaphore_Com = 96, // Semaphore Communications Sec. Pro
        EtherIP = 97, // ???
        Encap_Head = 98, // Encapsulation Header
        AnyPrivateEnc = 99, // Any private encryption scheme
        GMTP = 100, // ???
        IFMP = 101, // Ipsilon Flow Management Protocol
        PNNI_over_IP = 102, // ???
        PIM = 103, // Protocol Independent Multicast
        ARIS = 104, // ???
        SCPS = 105, // ???
        QNX = 106, // ???
        AN = 107, // Active Networks
        IPPCP = 108, // IP Payload Compression Protocol
        SNP = 109, // Sitara Networks Protocol
        Compaq_Peer = 110, // Compaq Peer Protocol
        IPX_in_IP = 111, // IPX in IP
        VRRP = 112, // Virtual Router Redundancy Protocol
        PGM = 113, // Pragmatic General Multicast
        AnyZeroHop = 114, // Any 0-hop protocol
        L2TP = 115, // Level 2 Tunneling Protocol
        DDX = 116, // D-II Data Exchange
        IATP = 117, // Interactive Agent Transfer Protocol
        ST = 118, // Schedule Transfer
        SRP = 119, // SpectraLink Radio Protocol
        UTI = 120, // ???
        SMP = 121, // Simple Message Protocol
        SM = 122, // ???
        PTP = 123, // Performance Transparency Protocol
        ISIS_over_IPv4 = 124, // ISIS over IPv4
        FIRE = 125, // ???
        CRTP = 126, // Combat Radio Transport Protocol
        CRUDP = 127, // Combat Radio User Datagram
        SSCOPMCE = 128, // ???
        IPLT = 129, // ???
        SPS = 130, // Secure Packet Shield
        PIPE = 131, // Private IP Encapsulation within IP
        SCTP = 132, // Stream Control Transmission Protocol
        Fibre = 133, // Fibre Channel
        RSVP_E2E_IGN = 134, // RSVP-E2E-IGNORE
        Mobility_Head = 135, // Mobility Header
        UDP_Lite = 136, // Lightweight User Datagram Protocol
        MPLS_in_IP = 137 // MPLS-in-IP
    }

    [Serializable]
    public enum IpVersion
    {
        IP = 4,		// IP, Internet Protocol.
        ST = 5,		// ST, ST Datagram Mode.
        SIP = 6,	// SIP, Simple Internet Protocol.
        TPIX = 7,	// TP/IX, The Next Internet.
        PIP = 8,	// PIP, The P Internet Protocol.
        TUBA = 9	// TUBA.
    }

    /// <summary>
    /// Contains information about sender/receiver IP addresses
    /// </summary>
    [Serializable]
    public class IpHeader
    {
        private uint ipSource;
        private uint ipDestination;
        private const int ADDRESS_LENGTH = 4;
        public const string UNKNOWN_HOST = "Unknown";

        /// <summary>
        /// Parses the data of the raw packet
        /// </summary>
        /// <param name="rawPacket">The raw packet (binary data)</param>
        /// <param name="macHeader">Lower-level information about the packet</param>
        public IpHeader(RawPacket rawPacket, EthernetHeader macHeader)
        {
            int pos = macHeader.Length;
            Version = (IpVersion)(rawPacket.Data[pos++] >> 4);

            int protocolOffset = pos + 8;
            Protocol = (IpProtocol)rawPacket.Data[protocolOffset++];

            int sourceOffset = protocolOffset + 2;
            ipSource = RawPacket.ReadUInt32(rawPacket.Data, sourceOffset);

            int destinationOffset = sourceOffset + 4;
            ipDestination = RawPacket.ReadUInt32(rawPacket.Data, destinationOffset);
        }

        /// <summary>
        /// Converts an uint to string
        /// </summary>
        /// <param name="address">uint representaion of the address</param>
        /// <returns>string representation of the address</returns>
        public static string UintIpAddressToString(uint address)
        {
            string formattedAddress = "";
            int shift = 24;

            for (int i = 0; i < ADDRESS_LENGTH; i++)
            {
                byte b = (byte)((address >> shift) & 0xFF);
                formattedAddress += b.ToString() + (i != 3 ? "." : "");
                shift -= 8;
            }

            return formattedAddress;
        }

        public string SourceIpAddress
        {
            get { return UintIpAddressToString(ipSource); }
        }

        public string DestinationIpAddress
        {
            get { return UintIpAddressToString(ipDestination); }
        }

        public string DestinationHostName
        {
            get 
            {
                string dstHostName = "";
                try
                {
                    dstHostName = Dns.GetHostEntry(DestinationIpAddress).HostName;
                }
                catch (Exception)
                {
                    dstHostName = UNKNOWN_HOST;
                }

                return dstHostName;
            }
        }

        public string  SourceHostName
        {
            get
            {
                string srcHostName = "";
                try
                {
                    srcHostName = Dns.GetHostEntry(SourceIpAddress).HostName;
                }
                catch (Exception)
                {
                    srcHostName = UNKNOWN_HOST;
                }

                return srcHostName;
            }
        }

        public IpProtocol Protocol { get; private set; }
        public IpVersion Version { get; private set; }
    }
}
