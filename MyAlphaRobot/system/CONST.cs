using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot
{
    public static class CONST
    {
        public static int DEFAULT_TRY_COUNT = 3;

        public static string CONFIG_FILE = "system.json";
        public static string DEFAULT_ROBOT_CONFIG = "robot.jaz";
        public static string ROBOT_CONFIG_FILTER = "Robot Config|*.jaz";
        public static string ROBOT_ACTION_FILTER = "Robot Action|*.myAct";
        public static string IMAGE_FILTER = "JPG, PNG|*.jpg, *.png|AL (*.*)|*.*";

        public static int MAX_SERVO = 32;
        public static int MAX_ROBOT_SERVO = 32;
        public const int SERVO_SIZE = 38;
        public const int SERVO_TIME_FACTOR = 20;  // 20 or 25??

        public enum UBT_FILE
        {
            AESX, HTS, UNKNOWN
        }

        // Combo Information
        public static class CI
        {
            public const int MAX_COMBO = 10;
            public const int MAX_COMBO_SIZE = 20;
            public const int COMBO_SIZE = 2;
            public static class OFFSET
            {
                public const int ID = 4;
                public const int COMBO_DATA = 10;
            }
        }

        // Action Information
        public static class AI
        {
            public const int MAX_ACTION = 250;
            public const int MAX_POSES = 65535;
            public const int ACTION_SIZE = HEADER.SIZE + MAX_POSES * POSE.SIZE;

            public const byte STOP_MUSIC_VOL = 0xFE;

            public const byte FULL_DATA_OFFSET = 8;

            public static class HEADER
            {
                public const int SIZE = 60;
                public static class OFFSET
                {
                    public const int ID = 4;
                    public const int NAME = 6;
                    public const int NAME_LEN = 27;
                    public const int POSE_CNT_LOW = 28;
                    public const int POSE_CNT_HIGH = 29;
                    public const int EXECUTE_TIME = 30;
                    public const int AFFECT_SERVO = 34;
                    public const int CHECKSUM = 58;
                }
            }
            public static class POSE
            {
                public const int SIZE = 60;
                public static class OFFSET
                {
                    public const int ID = 4;
                    public const int SEQ_LOW = 5;
                    public const int SEQ_HIGH = 55;
                    public const int ENABLE = 6;
                    public const int EXECUTE_TIME = 7;
                    public const int WAIT_TIME = 9;
                    public const int ANGLE_ID = 10;    // ID starting from 1, to simplify the operation, use position - 1
                    public const int SERVO_ANGLE = 11;
                    public const int SERVO_LED = 43;
                    public const int HEAD_LED = 51;
                    public const int MP3_FOLDER = 52;
                    public const int MP3_FILE = 53;
                    public const int MP3_VOL = 54;
                    public const int CHECKSUM = 58;
                }
            }
        }

        public static class V1CMD
        {
            public const char GET_ANGLE = 'A';
            public const char DEBUG_ENBLE = 'B';
            public const char DEBUG_DISABLE = 'b';
            public const char DOWNLOAD = 'D';
            public const char UPLOAD = 'U';
            public const char READ_SPIFFS = 'R';
            public const char WRITE_SPIFFS = 'W';
            public const char LOCK_SERVO = 'L';
            public const char FREE_SERVO = 'F';
            public const char MOVE_SERVO = 'M';
            public const char PLAY = 'P';
            public const char DETECT_SERVO = 'T';
            public const char RESET_CONN = 'Z';
        }

        public static class AESX_FILE
        {
            public const int HEADER_SIZE = 153;
            public const int RECORD_SIZE = 216;
            public const int TAIL_SIZE = 10;
            public const int MIN_SIZE = HEADER_SIZE + RECORD_SIZE + TAIL_SIZE;
            public const byte POSE_START = 0xD4;
            public const int OFFSET_RUNTIME = 12;
            public const int OFFSET_ALLTIME = 16;
            public const int OFFSET_ANGLE = 92;
            public const int MAX_SERVO = 16;
            public const int SERVO_SIZE = 8;

            public const int MS_PER_TIME = 20;

        }

        public static class HTS_FILE
        {
            public const int HEADER_SIZE = 33;
            public const int RECORD_SIZE = 33;
            public const int TAIL_SIZE = 33;
            public const int MIN_SIZE = HEADER_SIZE + RECORD_SIZE + TAIL_SIZE;
            public const byte POSE_START_01 = 0xFB;
            public const byte POSE_START_02 = 0xBF;
            public const byte POSE_END = 0xED;
            public const int MAX_SERVO = 16;  // should be 20, but the last 4 is always 0x5A, ignore them 
            public const int SERVO_SIZE = 1;
            public static class OFFSET
            {
                public const int POSE_ID = 6;
                public const int SERVO_ANGLE = 8;
                public const int SERVO_TIME = 28;
                public const int WAIT_TIME = 29;

            }
        }

        public static class SERVO
        {
            public const int MAX_ANGLE = 240;
        }

        public static class LED
        {
            public const byte NO_CHANGE = 0b00;
            public const byte TURN_ON = 0b10;
            public const byte TURN_OFF = 0b11;
        }

        // V2 command set
        public static class CMD
        {
            public const byte RESET = 0x01;
            public const byte DEBUG = 0x02;
            public const byte DEVMODE = 0x03;
            public const byte GET_CONFIG = 0x04;
            public const byte SET_CONFIG = 0x05;
            public const byte DEFAULT_CONFIG = 0x06;
            public const byte ENABLE = 0x0A;
            public const byte CHECKBATTERY = 0x0B;
            public const byte GETNETWORK = 0x0C;
            public const byte SERVOANGLE = 0x11;
            public const byte ONEANGLE = 0x12;
            public const byte SERVOADJANGLE = 0x13;
            public const byte ONEADJANGLE = 0x14;
            public const byte SETADJANGLE = 0x15;
            public const byte LOCKSERVO = 0x21;
            public const byte UNLOCKSERVO = 0x22;
            public const byte SERVOMOVE = 0x23;
            public const byte LED = 0x24;
            public const byte HEAD_LED = 0x31;
            public const byte PLAYACTION = 0x41;
            public const byte STOPACTION = 0x4F;

            public static class MP3
            {
                public const byte Stop = 0x32;
                public const byte PlayFile = 0x33;
                public const byte PlayMP3 = 0x34;
                public const byte PlayAdvert = 0x35;
                public const byte SetVol = 0x36;
            }
            public const byte GET_ADLIST = 0x60;
            public const byte GET_ADLIST_RETURN = 38;

            public const byte GET_ADHEADER = 0x61;
            public const byte GET_ADPOSE = 0x62;
            // public const byte GET_ADDATA = 0x63;

            public const byte GET_COMBO = 0x68;
            public const byte UPD_COMBO = 0x69;


            public const byte UPD_ADHEADER = 0x71;
            public const byte UPD_ADPOSE = 0x72;
            public const byte UPD_ADNAME = 0x74;
            public const byte DEL_ACTION = 0x75;
            // public const byte READSPIFFS = 0xF1;
            // public const byte WRITESPIFFS = 0xF2;

            public static class RETURN_LEN
            {
                public const byte GET_CONFIG = 56;
                public const byte GET_COMBO = 56;
                public const byte GET_ADLIST = 34;
            }

        }

    }
}
