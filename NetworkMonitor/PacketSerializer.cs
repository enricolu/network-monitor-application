using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NetworkMonitor
{
    public class PacketSerializer
    {
        private string packetDatabase = "packets.bin";

        public PacketSerializer(string fileName)
        {
            this.packetDatabase = fileName;
        }

        public void SerializePacket(Packet packet)
        {
            using (FileStream fileStream = new FileStream(packetDatabase, FileMode.Append, FileAccess.Write, FileShare.Write))
            {
                MemoryStream compressedPacketStream = new MemoryStream();
                Packet.CompressPacket(packet, out compressedPacketStream);

                BinaryWriter writer = new BinaryWriter(fileStream);
                writer.Write(compressedPacketStream.Capacity);
                compressedPacketStream.WriteTo(fileStream);

                compressedPacketStream.Close();
                writer.Close();
            }
        }

        public List<Packet> DeserializeAllPackets()
        {
            FileStream fileStream = new FileStream(packetDatabase, FileMode.Open);
            List<Packet> packets = new List<Packet>();

            BinaryReader binaryReader = new BinaryReader(fileStream);
            while (fileStream.Position <= fileStream.Length - 1)
            {
                int packetSize = binaryReader.ReadInt32();

                if (packetSize != 0)
                {
                    byte[] packetData = binaryReader.ReadBytes(packetSize);
                    MemoryStream packetStream = new MemoryStream();
                    Packet.DecompressPacket(packetData, out packetStream);
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    Packet p = (Packet)binaryFormatter.Deserialize(packetStream);
                    packets.Add(p);
                }
            }

            return packets;
        }
    }
}
