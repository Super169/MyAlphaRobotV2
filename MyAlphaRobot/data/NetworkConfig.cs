using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{

    public class NetworkConfig
    {
        private const byte DATA_SIZE = 120;
        private const byte CONFIG_DATA_SIZE = 116;

        private static class OFFSET
        {
            public const byte VER_MAJOR = 4;
            public const byte VER_MINOR = 5;
            public const byte ENABLE_ROUTER = 8;
            public const byte SSID = 10;
            public const byte PASSWORD = 40;
            public const byte ROUTER_TIMEOUT = 60;
            public const byte ENABLE_AP = 62;
            public const byte AP_NAME = 64;
            public const byte AP_KEY = 84;
            public const byte ENABLE_SERVER = 104;
            public const byte SERVER_PORT = 106;
            public const byte ENABLE_UDP = 108;
            public const byte UDP_RX_PORT = 110;
            public const byte UDP_TX_PORT = 112;
            public const byte CHECKSUM = 118;
            public const byte END_BYTE = 119;
        };

        public byte[] data = new byte[DATA_SIZE];

        // Note, this one is different from BoardConfig
        private UInt16 getUInt16(byte offset)
        {
            return (UInt16)(data[offset + 1] << 8 | data[offset]);
        }

        private void setUInt16(byte offset, UInt16 value)
        {
            data[offset + 1] = (byte)(value / 256);
            data[offset] = (byte)(value & 0xFF);
        }

        private String getString(byte offset, byte len)
        {
            List<byte> buffer = new List<byte>();
            for (int i = 0; i < len; i++)
            {
                if (this.data[offset + i] == 0) break;
                buffer.Add(this.data[offset + i]);
            }
            return System.Text.Encoding.ASCII.GetString(buffer.ToArray());
        }

        private void setString(byte offset, String value, byte len)
        {
            char[] aValue = value.ToCharArray();
            int ptr = offset;
            for (int i = 0; i < 30; i++) this.data[offset + i] = 0;

            for (int i = 0; i < 30; i++)
            {
                if (i >= aValue.Length)
                {
                    break;
                }
                else
                {
                    // Skip control character if any
                    if (aValue[i] >=  20) {
                        this.data[ptr++] = (byte)aValue[i];
                    }
                }
            }
        }

        public NetworkConfig()
        {

        }

        public bool enableRouter
        {
            get { return (this.data[OFFSET.ENABLE_ROUTER] == 1); }
            set { this.data[OFFSET.ENABLE_ROUTER] = (byte)(value ? 1 : 0); }
        }

        public String ssid
        {
            get { return getString(OFFSET.SSID, 30); }
            set
            { setString(OFFSET.SSID, value, 30); }
        }

        public String password
        {
            get { return getString(OFFSET.PASSWORD, 20); }
            set
            { setString(OFFSET.PASSWORD, value, 20); }
        }

        public byte routerTimeout
        {
            get { return this.data[OFFSET.ROUTER_TIMEOUT]; }
            set { this.data[OFFSET.ROUTER_TIMEOUT] = value; }
        }

        public bool enableAP
        {
            get { return (this.data[OFFSET.ENABLE_AP] == 1); }
            set { this.data[OFFSET.ENABLE_AP] = (byte)(value ? 1 : 0); }
        }

        public String apName
        {
            get { return getString(OFFSET.AP_NAME, 20); }
            set
            { setString(OFFSET.AP_NAME, value, 20); }
        }

        public String apKey
        {
            get { return getString(OFFSET.AP_KEY, 20); }
            set
            { setString(OFFSET.AP_KEY, value, 20); }
        }

        public bool enableServer
        {
            get { return (this.data[OFFSET.ENABLE_SERVER] == 1); }
            set { this.data[OFFSET.ENABLE_SERVER] = (byte)(value ? 1 : 0); }
        }

        public UInt16 serverPort
        {
            get { return getUInt16(OFFSET.SERVER_PORT); }
            set { setUInt16(OFFSET.SERVER_PORT, value); }
        }

        public bool enableUDP
        {
            get { return (this.data[OFFSET.ENABLE_UDP] == 1); }
            set { this.data[OFFSET.ENABLE_UDP] = (byte)(value ? 1 : 0); }
        }

        public UInt16 udpRxPort
        {
            get { return getUInt16(OFFSET.UDP_RX_PORT); }
            set { setUInt16(OFFSET.UDP_RX_PORT, value); }
        }

        public UInt16 udpTxPort
        {
            get { return getUInt16(OFFSET.UDP_TX_PORT); }
            set { setUInt16(OFFSET.UDP_TX_PORT, value); }
        }


        public bool ReadFromArray(byte[] data)
        {
            if ((data.Length >= DATA_SIZE) &&
                (data[0] == 0xA9) && (data[1] == 0x9A) && (data[CONFIG_DATA_SIZE + 3] == 0xED) &&
                (data[2] == CONFIG_DATA_SIZE))
            {
                for (int i = 0; i < DATA_SIZE; i++) { this.data[i] = data[i]; }
                return true;
            }
            return false;
        }
    }
}
