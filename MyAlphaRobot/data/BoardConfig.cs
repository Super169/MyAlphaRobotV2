using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{

    public class BoardConfig
    {
        private const byte DATA_SIZE = 60;
        private const byte CONFIG_DATA_SIZE = 56;
        private const byte MAX_TOUCH_ACTION = 4;
        private const byte TOUCH_NO_ACTION = 0;

        private static class OFFSET
        {
            public const byte VERSION = 04;
            public const byte ENABLE_DEBUG = 05;
            public const byte CONNECT_ROUTER = 06;
            public const byte ENABLE_OLED = 07;
            public const byte ENABLE_TOUCH = 08;
            public const byte REF_VOLTAGE = 10;
            public const byte MIN_VOLTAGE = 12;
            public const byte MAX_VOLTAGE = 14;
            public const byte ALARM_VOLTAGE = 16;
            public const byte ALARM_MP3 = 18;
            public const byte ALARM_INTERVAL = 19;
            public const byte MAX_SERVO = 21;
            public const byte MAX_DETECT_RETRY = 22;
            public const byte MAX_COMMAND_WAIT_MS = 23;
            public const byte MAX_COMMAND_RETRY = 24;
            public const byte MP3_ENABLED = 31;
            public const byte MP3_VOLUME = 32;
            public const byte MP3_STARTUP = 33;
            public const byte AUTO_STAND = 41;
            public const byte AUTO_FACE_UP = 42;
            public const byte AUTO_FACE_DOWN = 43;
            public const byte TOUCH_ACTION = 44;
            public const byte TOUCH_DETECT_PERIOD = 48;
            public const byte TOUCH_RELEASE_PERIOD = 50;
            public const byte MPU_CHECK_FREQ = 52;
            public const byte POSITION_CHECK_FREQ = 53;
        };

        public byte[] data = new byte[DATA_SIZE];

        private UInt16 getUInt16(byte offset)
        {
            return (UInt16) (data[offset] << 8 | data[offset+1]);
        }

        private void setUInt16(byte offset, UInt16 value)
        {
            data[offset] = (byte)(value / 256);
            data[offset + 1] = (byte)(value & 0xFF);
        }

        public bool enableDebug
        {
            get { return (this.data[OFFSET.ENABLE_DEBUG] == 1); }
            set { this.data[OFFSET.ENABLE_DEBUG] = (byte)(value ? 1 : 0); }
        }

        public bool connectRouter
        {
            get { return (data[OFFSET.CONNECT_ROUTER] == 1); }
            set { data[OFFSET.CONNECT_ROUTER] = (byte)(value ? 1 : 0); }
        }

        public bool enableOLED
        {
            get { return (this.data[OFFSET.ENABLE_OLED] == 1); }
            set { this.data[OFFSET.ENABLE_OLED] = (byte)(value ? 1 : 0); }
        }

        public bool enableTouch
        {
            get { return (this.data[OFFSET.ENABLE_TOUCH] == 1); }
            set { this.data[OFFSET.ENABLE_TOUCH] = (byte)(value ? 1 : 0); }
        }

        public UInt16 touchDetectPeriod
        {
            get { return getUInt16(OFFSET.TOUCH_DETECT_PERIOD); }
            set { setUInt16(OFFSET.TOUCH_DETECT_PERIOD, value); }
        }

        public UInt16 touchReleasePeriod
        {
            get { return getUInt16(OFFSET.TOUCH_RELEASE_PERIOD); }
            set { setUInt16(OFFSET.TOUCH_RELEASE_PERIOD, value); }
        }

        public UInt16 voltageRef
        {
            get { return getUInt16(OFFSET.REF_VOLTAGE); }
            set { setUInt16(OFFSET.REF_VOLTAGE, value); }
        }

        public UInt16 voltageLow
        {
            get { return getUInt16(OFFSET.MIN_VOLTAGE); }
            set { setUInt16(OFFSET.MIN_VOLTAGE, value); }
        }

        public UInt16 voltageHigh
        {
            get { return getUInt16(OFFSET.MAX_VOLTAGE); }
            set { setUInt16(OFFSET.MAX_VOLTAGE, value); }
        }

        public UInt16 voltageAlarm
        {
            get { return getUInt16(OFFSET.ALARM_VOLTAGE); }
            set { setUInt16(OFFSET.ALARM_VOLTAGE, value); }
        }

        public byte voltageAlarmMp3
        {
            get { return data[OFFSET.ALARM_MP3]; }
            set { data[OFFSET.ALARM_MP3] = value; }
        }

        public byte voltageAlarmInterval
        {
            get { return data[OFFSET.ALARM_INTERVAL]; }
            set { data[OFFSET.ALARM_INTERVAL] = value; }
        }

        public byte maxServo
        {
            get { return data[OFFSET.MAX_SERVO]; }
            set { data[OFFSET.MAX_SERVO] = value; }
        }

        public byte maxDetectRetry
        {
            get { return data[OFFSET.MAX_DETECT_RETRY]; }
            set { data[OFFSET.MAX_DETECT_RETRY] = value; }
        }

        public byte commandTimeout
        {
            get { return data[OFFSET.MAX_COMMAND_WAIT_MS]; }
            set { data[OFFSET.MAX_COMMAND_WAIT_MS] = value; }
        }

        public byte maxCommandRetry
        {
            get { return data[OFFSET.MAX_COMMAND_RETRY]; }
            set { data[OFFSET.MAX_COMMAND_RETRY] = value; }
        }

        public bool enableMp3
        {
            get { return (this.data[OFFSET.MP3_ENABLED] == 1); }
            set { this.data[OFFSET.MP3_ENABLED] = (byte)(value ? 1 : 0); }
        }


        public byte mp3Volume
        {
            get { return data[OFFSET.MP3_VOLUME]; }
            set { data[OFFSET.MP3_VOLUME] = value; }
        }

        public byte mp3Startup
        {
            get { return data[OFFSET.MP3_STARTUP]; }
            set { data[OFFSET.MP3_STARTUP] = value; }
        }


        public bool autoStand
        {
            get { return (this.data[OFFSET.AUTO_STAND] == 1); }
            set { this.data[OFFSET.AUTO_STAND] = (byte)(value ? 1 : 0); }
        }

        public byte autoStandFaceUp
        {
            get { return data[OFFSET.AUTO_FACE_UP]; }
            set { data[OFFSET.AUTO_FACE_UP] = value; }
        }

        public byte autoStandFaceDown
        {
            get { return data[OFFSET.AUTO_FACE_DOWN]; }
            set { data[OFFSET.AUTO_FACE_DOWN] = value; }
        }


        public byte touchAction(byte id)
        {
            if (id >= MAX_TOUCH_ACTION) return TOUCH_NO_ACTION;
            return data[OFFSET.TOUCH_ACTION + id];
        }

        public bool setTouchAction(byte id, byte action)
        {
            if (id >= MAX_TOUCH_ACTION) return false;
            data[OFFSET.TOUCH_ACTION + id] = action;
            return true;
        }

        public byte mpuCheckFreq
        {
            get { return data[OFFSET.MPU_CHECK_FREQ]; }
            set { data[OFFSET.MPU_CHECK_FREQ] = value; }
        }

        public byte positionCheckFreq
        {
            get { return data[OFFSET.POSITION_CHECK_FREQ]; }
            set { data[OFFSET.POSITION_CHECK_FREQ] = value; }
        }


        public BoardConfig()
        {

        }

        public bool ReadFromArray(byte[] data)
        {
            if ((data.Length >= DATA_SIZE) && 
                (data[0] == 0xA9) && (data[1] == 0x9A) && (data[59]==0xED) &&
                (data[2] == CONFIG_DATA_SIZE))
            {
                for (int i = 0; i < DATA_SIZE; i++) { this.data[i] = data[i];  }
                return true;
            }
            return false;
        }

    }
}
