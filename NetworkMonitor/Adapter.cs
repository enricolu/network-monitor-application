using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkMonitor
{
    /// <summary>
    /// Represents a network adapter 
    /// </summary>
    [Serializable]
    public class Adapter
    {
        public Adapter(string name, UInt32 id)
        {
            AdapterName = name;
            AdapterID = id;
        }

        public string AdapterName {get; private set; }
        public UInt32 AdapterID { get; private set; }
    }
}
