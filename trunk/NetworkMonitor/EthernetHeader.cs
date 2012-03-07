using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkMonitor
{
    /// <summary>
    /// Contains lowe-level information about the packet
    /// </summary>
    [Serializable]
    public class EthernetHeader
    {
        private const int DEFAULT_LENGTH = 14;
        private const int ADDRESS_LENGTH = 6;
        private byte[] macDestination = new byte[6];
        private byte[] macSource = new byte[6];

        /// <summary>
        /// Parses the raw binary data
        /// </summary>
        /// <param name="data">raw data</param>
        public EthernetHeader(byte[] data)
        {
            if (data == null || data.Length < DEFAULT_LENGTH)
            {
                throw new Exception("Invalid data array passed");
            }

            for (int i = 0; i < ADDRESS_LENGTH; i++)
            {
                macDestination[i] = data[i];
                macSource[i] = data[i + ADDRESS_LENGTH];
            }
        }

        /// <summary>
        /// Converts the MAC address to a string
        /// </summary>
        /// <param name="address">binary representation of the address</param>
        /// <returns>string representation of the address</returns>
        public static string MacAddressToString(byte[] address)
        {
            if (address == null || address.Length != ADDRESS_LENGTH)
            {
                throw new Exception("Invalid MAC address passed");
            }

            string formattedAddress = "";
            for (int i = 0; i < ADDRESS_LENGTH; i++)
            {
                string addressPart = address[i].ToString("X");
                if (addressPart.Length == 1)
                {
                    addressPart = "0" + addressPart;
                }

                formattedAddress += addressPart;
            }

            return formattedAddress;
        }

        public string SourceMacAddress
        {
            get { return MacAddressToString(macSource); }
        }

        public string DestinationMacAddress
        {
            get { return MacAddressToString(macSource); }
        }

        public int Length
        {
            get { return DEFAULT_LENGTH; }
        }
    }
}
