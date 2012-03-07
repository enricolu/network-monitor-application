using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkMonitor
{
    /// <summary>
    /// Low-level packet information (array of bytes)
    /// </summary>
    [Serializable]
    public class RawPacket
    {
        protected byte[] data;

        public RawPacket(byte[] data)
        {
            this.data = data;
        }

        public byte[] Data
        {
            get { return this.data; }
        }

        public int Size
        {
            get { return this.data.Length; }
        }

        /// <summary>
        /// Converts a byte array to 16-bit uint
        /// </summary>
        /// <param name="byteArray">the array to convert</param>
        /// <param name="pos">starting position</param>
        /// <returns>the uint</returns>
        public static UInt16 ReadUInt16(byte[] byteArray, int pos)
        {
            return (UInt16) ((byteArray[pos] << 8) | byteArray[pos + 1]);
        }

        /// <summary>
        /// Converts a byte array to 32-bit uint
        /// </summary>
        /// <param name="byteArray">the array to convert</param>
        /// <param name="pos">the starting position</param>
        /// <returns>the uint</returns>
        public static UInt32 ReadUInt32(byte[] byteArray, int pos)
        {
            return (UInt32) (
                                (byteArray[pos + 0] << 24) |
                                (byteArray[pos + 1] << 16) |
                                (byteArray[pos + 2] << 8 ) |
                                (byteArray[pos + 3] << 0 )
                            );
        }
    }
}
