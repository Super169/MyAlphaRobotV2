using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{

    public class BoardConfig
    {
        public const byte MIN_VERSION = 1;

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

            public const byte BATTERY_REF_VOLTAGE = 10;
            public const byte BATTERY_MIN_VALUE = 12;
            public const byte BATTERY_MAX_VALUE = 14;
            public const byte BATTERY_CHECK_SEC = 18;
            public const byte BATTERY_ALARM_SEC = 19;

            public const byte MAX_SERVO = 21;
            public const byte MAX_DETECT_RETRY = 22;
            public const byte MAX_COMMAND_WAIT_MS = 23;
            public const byte MAX_COMMAND_RETRY = 24;


            public const byte MP3_ENABLED = 31;
            public const byte MP3_VOLUME = 32;
            public const byte MP3_STARTUP = 33;
            public const byte STARTUP_ACTION = 34;

            public const byte PSX_ENABLED = 35;
            public const byte PSX_CHECK_MS = 36;
            public const byte PSX_NO_EVENT_MS = 37;
            public const byte PSX_IGNORE_REPEAT_MS = 38;
            public const byte PSX_SHOCK = 40;

            public const byte MPU_ENABLED = 41;
            public const byte MPU_CHECK_FREQ = 42;
            public const byte MPU_POSITION_CHECK_FREQ =43;

            public const byte TOUCH_ENABLED = 47;
            public const byte TOUCH_DETECT_PERIOD = 48;
            public const byte TOUCH_RELEASE_PERIOD = 50;

            public const byte SONIC_ENABLED = 53;
            public const byte SONIC_CHECK_FREQ = 54;
            public const byte SONIC_DELAY_SEC = 55;

            public const byte MAZE_SERVO = 56;
            public const byte MAZE_WALL_DISTANCE = 57;

            public const byte MAZE_SERVO_DIRECTION = 25;
            public const byte MAZE_SERVO_MOVE_MS = 26;
            public const byte MAZE_SERVO_WAIT_MS = 28;

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

        public byte version
        {
            get { return this.data[OFFSET.VERSION]; }
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


        public UInt16 batteryRefVoltage
        {
            get { return getUInt16(OFFSET.BATTERY_REF_VOLTAGE); }
            set { setUInt16(OFFSET.BATTERY_REF_VOLTAGE, value); }
        }

        public UInt16 batteryMinValue
        {
            get { return getUInt16(OFFSET.BATTERY_MIN_VALUE); }
            set { setUInt16(OFFSET.BATTERY_MIN_VALUE, value); }
        }

        public UInt16 batteryMaxValue
        {
            get { return getUInt16(OFFSET.BATTERY_MAX_VALUE); }
            set { setUInt16(OFFSET.BATTERY_MAX_VALUE, value); }
        }

        public byte batteryCheckSec
        {
            get { return data[OFFSET.BATTERY_CHECK_SEC]; }
            set { data[OFFSET.BATTERY_CHECK_SEC] = value; }
        }

        public byte batteryAlarmSec
        {
            get { return data[OFFSET.BATTERY_ALARM_SEC]; }
            set { data[OFFSET.BATTERY_ALARM_SEC] = value; }
        }



        #region Touch Sensor related

        public bool touchEnabled
        {
            get { return (this.data[OFFSET.TOUCH_ENABLED] == 1); }
            set { this.data[OFFSET.TOUCH_ENABLED] = (byte)(value ? 1 : 0); }
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

        #endregion


        #region "Sonic Sensor"

        public bool sonicEnabled
        {
            get { return (this.data[OFFSET.SONIC_ENABLED] == 1); }
            set { this.data[OFFSET.SONIC_ENABLED] = (byte)(value ? 1 : 0); }
        }

        public byte sonicCheckFreq
        {
            get { return this.data[OFFSET.SONIC_CHECK_FREQ];  }
            set { this.data[OFFSET.SONIC_CHECK_FREQ] = value;  }
        }

        public byte sonicDelaySec
        {
            get { return this.data[OFFSET.SONIC_DELAY_SEC]; }
            set { this.data[OFFSET.SONIC_DELAY_SEC] = value; }
        }

        #endregion

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

        public bool mp3Enabled
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

        public byte startupAction
        {
            get { return data[OFFSET.STARTUP_ACTION]; }
            set { data[OFFSET.STARTUP_ACTION] = value; }
        }

        public bool mpuEnabled
        {
            get { return (this.data[OFFSET.MPU_ENABLED] == 1); }
            set { this.data[OFFSET.MPU_ENABLED] = (byte)(value ? 1 : 0); }
        }

        public byte mpuCheckFreq
        {
            get { return data[OFFSET.MPU_CHECK_FREQ]; }
            set { data[OFFSET.MPU_CHECK_FREQ] = value; }
        }

        public byte mpuPositionCheckFreq
        {
            get { return data[OFFSET.MPU_POSITION_CHECK_FREQ]; }
            set { data[OFFSET.MPU_POSITION_CHECK_FREQ] = value; }
        }

        public bool psxEnabled
        {
            get { return (this.data[OFFSET.PSX_ENABLED] == 1); }
            set { this.data[OFFSET.PSX_ENABLED] = (byte)(value ? 1 : 0); }
        }

        public byte psxCheckMs
        {
            get { return this.data[OFFSET.PSX_CHECK_MS]; }
            set { this.data[OFFSET.PSX_CHECK_MS] =  value; }
        }

        public byte psxNoEventMs
        {
            get { return this.data[OFFSET.PSX_NO_EVENT_MS]; }
            set { this.data[OFFSET.PSX_NO_EVENT_MS] = value; }
        }

        public UInt16 psxIgnoreRepeatMs
        {
            get { return getUInt16(OFFSET.PSX_IGNORE_REPEAT_MS); }
            set { setUInt16(OFFSET.PSX_IGNORE_REPEAT_MS, value); }
        }

        public bool psxShock
        {
            get { return (this.data[OFFSET.PSX_SHOCK] == 1); }
            set { this.data[OFFSET.PSX_SHOCK] = (byte)(value ? 1 : 0); }
        }

        public byte mazeServo
        {
            get { return data[OFFSET.MAZE_SERVO]; }
            set { this.data[OFFSET.MAZE_SERVO] = value; }
        }

        public byte mazeWallDistance
        {
            get { return data[OFFSET.MAZE_WALL_DISTANCE]; }
            set { data[OFFSET.MAZE_WALL_DISTANCE] = value; }
        }

        public bool mazeServoReverseDirection
        {
            get { return (data[OFFSET.MAZE_SERVO_DIRECTION] != 0); }
            set { data[OFFSET.MAZE_SERVO_DIRECTION] = (byte) (value ? 1 : 0); }
        }

        public UInt16 mazeServoMoveMs
        {
            get { return getUInt16(OFFSET.MAZE_SERVO_MOVE_MS); }
            set { setUInt16(OFFSET.MAZE_SERVO_MOVE_MS, value); }
        }

        public UInt16 mazeServoWaitMs
        {
            get { return getUInt16(OFFSET.MAZE_SERVO_WAIT_MS); }
            set { setUInt16(OFFSET.MAZE_SERVO_WAIT_MS, value); }
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
