using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class PoseInfo
    {
        public static int servoCnt = CONST.MAX_SERVO;

        public byte actionId { get; set; }
        public UInt16 poseId { get; set; }
        public bool enabled { get; set; }
        public byte[] servoAngle = new byte[servoCnt + 1];  // to sync with id, use id directly, so 17 item here.
        public byte[] servoLed = new byte[servoCnt + 1];
        public byte headLed = 0x00;
        public byte mp3Folder = 0xff;
        public byte mp3File = 0xff;
        public byte mp3Vol = 0xff;
        public UInt16 servoTime { get; set; }
        public UInt16 waitTime { get; set; }
        public string s01 { get { return ServoDisplayStr(1); } }
        public string s02 { get { return ServoDisplayStr(2); } }
        public string s03 { get { return ServoDisplayStr(3); } }
        public string s04 { get { return ServoDisplayStr(4); } }
        public string s05 { get { return ServoDisplayStr(5); } }
        public string s06 { get { return ServoDisplayStr(6); } }
        public string s07 { get { return ServoDisplayStr(7); } }
        public string s08 { get { return ServoDisplayStr(8); } }
        public string s09 { get { return ServoDisplayStr(9); } }
        public string s10 { get { return ServoDisplayStr(10); } }
        public string s11 { get { return ServoDisplayStr(11); } }
        public string s12 { get { return ServoDisplayStr(12); } }
        public string s13 { get { return ServoDisplayStr(13); } }
        public string s14 { get { return ServoDisplayStr(14); } }
        public string s15 { get { return ServoDisplayStr(15); } }
        public string s16 { get { return ServoDisplayStr(16); } }
        public string s17 { get { return ServoDisplayStr(17); } }
        public string s18 { get { return ServoDisplayStr(18); } }
        public string s19 { get { return ServoDisplayStr(19); } }
        public string s20 { get { return ServoDisplayStr(20); } }
        public string s21 { get { return ServoDisplayStr(21); } }
        public string s22 { get { return ServoDisplayStr(22); } }
        public string s23 { get { return ServoDisplayStr(23); } }
        public string s24 { get { return ServoDisplayStr(24); } }
        public string s25 { get { return ServoDisplayStr(25); } }
        public string s26 { get { return ServoDisplayStr(26); } }
        public string s27 { get { return ServoDisplayStr(27); } }
        public string s28 { get { return ServoDisplayStr(28); } }
        public string s29 { get { return ServoDisplayStr(29); } }
        public string s30 { get { return ServoDisplayStr(30); } }
        public string s31 { get { return ServoDisplayStr(31); } }
        public string s32 { get { return ServoDisplayStr(32); } }
        public string LED00 { get { return GetLED(0); } }
        public string LED01 { get { return GetLED(1); } }
        public string LED02 { get { return GetLED(2); } }
        public string LED03 { get { return GetLED(3); } }
        public string LED04 { get { return GetLED(4); } }
        public string LED05 { get { return GetLED(5); } }
        public string LED06 { get { return GetLED(6); } }
        public string LED07 { get { return GetLED(7); } }
        public string LED08 { get { return GetLED(8); } }
        public string LED09 { get { return GetLED(9); } }
        public string LED10 { get { return GetLED(10); } }
        public string LED11 { get { return GetLED(11); } }
        public string LED12 { get { return GetLED(12); } }
        public string LED13 { get { return GetLED(13); } }
        public string LED14 { get { return GetLED(14); } }
        public string LED15 { get { return GetLED(15); } }
        public string LED16 { get { return GetLED(16); } }
        public string LED17 { get { return GetLED(17); } }
        public string LED18 { get { return GetLED(18); } }
        public string LED19 { get { return GetLED(19); } }
        public string LED20 { get { return GetLED(20); } }
        public string LED21 { get { return GetLED(21); } }
        public string LED22 { get { return GetLED(22); } }
        public string LED23 { get { return GetLED(23); } }
        public string LED24 { get { return GetLED(24); } }
        public string LED25 { get { return GetLED(25); } }
        public string LED26 { get { return GetLED(26); } }
        public string LED27 { get { return GetLED(27); } }
        public string LED28 { get { return GetLED(28); } }
        public string LED29 { get { return GetLED(29); } }
        public string LED30 { get { return GetLED(30); } }
        public string LED31 { get { return GetLED(31); } }
        public string LED32 { get { return GetLED(32); } }

        public string mp3DispInfo
        {
            get
            {
                if (mp3Vol == CONST.AI.STOP_MUSIC_VOL)
                {
                    return "停止擋放";
                }
                if ((mp3Folder == 0xff) && (mp3File == 0xff))
                {
                    return "";
                }
                return String.Format("{0}{1:000}", (mp3Folder == 0xFF ? "" : string.Format("{0:00}\\", mp3Folder)), mp3File);
            }
        }

        public string mp3DispVol
        {
            get
            {
                return (mp3Vol > 30 ? "" : mp3Vol.ToString());
            }
        }

        private int[,] UBF_TIME = { {100,1092616192},
                                    {200,1101004800},
                                    {300,1106247680},
                                    {400,1109393408},
                                    {500,1112014848},
                                    {600,1114636288},
                                    {700,1116471296},
                                    {800,1117782016},
                                    {900,1119092736},
                                    {1000,1120403456},
                                    {1100,1121714176},
                                    {1200,1123024896},
                                    {1300,1124204544},
                                    {1400,1124859904},
                                    {1500,1125515264},
                                    {2000,1128792064}
                                     };

        private string ServoDisplayStr(byte id)
        {
            if (servoTime == 0) return "---";
            byte angle = this.servoAngle[id];
            if (angle > CONST.SERVO.MAX_ANGLE)
            {
                return "---";
            }
            else
            {
                return angle.ToString();
            }
        }

        private string GetLED(byte id)
        {
            // if ((id > 0) && (ServoDisplayStr(id) == "---")) return "Transparent";
            byte LED = (id == 0 ? headLed : servoLed[id]);
            switch (LED)
            {
                case CONST.LED.TURN_OFF:
                    return "Gray";
                case CONST.LED.TURN_ON:
                    return "YellowGreen";
            }
            return "Transparent";
        }


        private byte[] emptyPose = {0xA9, 0x9A, 0x38, 0x62, 0x00, 0x00, 0x01,
                                    0x00, 0x00,
                                    0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                    0x00,
                                    0xFF, 0xFF, 0xFF,
                                    0x00, 0x00, 0x00, 0x00, 0x00
                                   };

        public PoseInfo(byte actionId, UInt16 poseId)
        {
            this.actionId = actionId;
            this.poseId = poseId;
            this.Reset();
        }

        public void Reset()
        {
            this.enabled = false;
            for (int i = 1; i <= servoCnt; i++) servoAngle[i] = 0xFF;
            for (int i = 1; i <= servoCnt; i++) servoLed[i] = 0x00;
            this.servoTime = 0;
            this.waitTime = 0;
            this.headLed = CONST.LED.NO_CHANGE;
            this.mp3Folder = 0xff;
            this.mp3File = 0xff;
            this.mp3Vol = 0xff;
        }

        public bool Update(int actionId, int poseId, byte[] servoAngle, byte[] servoLed, byte headLed, byte mp3Folder, byte mp3File, byte mp3Vol, UInt16 servoTime, UInt16 waitTime)
        {
            if ((this.actionId != actionId) || (this.poseId != poseId)) return false;
            for (int id = 1; id <= servoCnt; id++)
            {
                this.servoAngle[id] = servoAngle[id];
                this.servoLed[id] = servoLed[id];
            }
            this.headLed = headLed;
            this.mp3Folder = mp3Folder;
            this.mp3File = mp3File;
            this.mp3Vol = mp3Vol;
            this.servoTime = servoTime;
            this.waitTime = waitTime;
            return true;
        }

        private long GetLong(byte[] actionData, int offset)
        {
            long value = 0;
            for (int i = 3; i >= 0; i--)
            {
                value = value * 256 + actionData[offset + i];
            }
            return value;
        }

        private long GetTimeMs(long data)
        {
            long value = -1;
            double rate;

            /// minimum known value 100
            if (data <= UBF_TIME[0, 1]) return UBF_TIME[0, 0];

            for (int i = 1; i < UBF_TIME.GetLength(0); i++)
            {
                if (UBF_TIME[i, 1] >= data)
                {
                    rate = (data - UBF_TIME[i - 1, 1]) / (UBF_TIME[i, 1] - UBF_TIME[i - 1, 1]);
                    value = (int)(UBF_TIME[i - 1, 0] + rate * (UBF_TIME[i, 0] - UBF_TIME[i - 1, 0]));
                    return value;
                }
            }

            rate = (data - UBF_TIME[UBF_TIME.Length - 1, 1]) / (UBF_TIME[UBF_TIME.Length - 1, 1] - UBF_TIME[UBF_TIME.Length - 2, 1]);
            value = (int)(UBF_TIME[UBF_TIME.Length - 1, 0] + rate * (UBF_TIME[UBF_TIME.Length - 1, 0] - UBF_TIME[UBF_TIME.Length - 2, 0]));
            return value;
        }

        public bool ReadFromAESX(byte[] actionData, int actionId, int poseId)
        {
            if ((this.actionId != actionId) || (this.poseId != poseId)) return false;
            int startPos = CONST.AESX_FILE.HEADER_SIZE + poseId * CONST.AESX_FILE.RECORD_SIZE;
            if ((actionData[startPos] != CONST.AESX_FILE.POSE_START) ||
                (actionData[startPos + 4] != CONST.AESX_FILE.POSE_START))
            {
                return false;
            }
            long servoTime = 0;
            long waitTime = 0;
            servoTime = GetLong(actionData, startPos + 12);
            waitTime = GetLong(actionData, startPos + 16);
            long servoTimeMs = GetTimeMs(servoTime);
            long waitTimeMs = GetTimeMs(waitTime);
            byte servoTimeByte = (byte)(servoTimeMs / CONST.AESX_FILE.MS_PER_TIME);

            this.enabled = true;
            int max_servo = Math.Min(CONST.AESX_FILE.MAX_SERVO, CONST.MAX_SERVO);
            for (int id = 1; id <= max_servo; id++)
            {
                servoAngle[id] = actionData[startPos + CONST.AESX_FILE.OFFSET_ANGLE + (id - 1) * CONST.AESX_FILE.SERVO_SIZE];
            }
            this.servoTime = (UInt16)servoTimeMs;
            this.waitTime = (UInt16)waitTimeMs;
            return true;
        }

        public bool ReadFromHTS(byte[] actionData, int actionId, int poseId)
        {
            if ((this.actionId != actionId) || (this.poseId != poseId)) return false;
            int startPos = CONST.HTS_FILE.HEADER_SIZE + poseId * CONST.HTS_FILE.RECORD_SIZE;

            // poseId in offset 6, but do not check at this moment; may allow to switch pose later
            if ((actionData[startPos] != CONST.HTS_FILE.POSE_START_01) ||
                (actionData[startPos + 1] != CONST.HTS_FILE.POSE_START_02) ||
                (actionData[startPos + CONST.HTS_FILE.RECORD_SIZE - 1] != CONST.HTS_FILE.POSE_END))
            {
                return false;
            }
            long servoTime = 0;
            long waitTime = 0;
            long servoTimeMs = 0;
            long waitTimeMs = 0;
            servoTime = actionData[startPos + CONST.HTS_FILE.OFFSET.SERVO_TIME];
            waitTime = (actionData[startPos + CONST.HTS_FILE.OFFSET.WAIT_TIME] << 8) | actionData[startPos + CONST.HTS_FILE.OFFSET.WAIT_TIME + 1];
            servoTimeMs = servoTime * 20;
            // waitTime start with 00 08 for 200ms, and 1 ~ 20s thereafter
            waitTimeMs = 200 + (waitTime - 8) * 20;
            this.enabled = true;

            int max_servo = Math.Min(CONST.HTS_FILE.MAX_SERVO, CONST.MAX_SERVO);
            for (int id = 1; id <= max_servo; id++)
            {
                servoAngle[id] = actionData[startPos + CONST.HTS_FILE.OFFSET.SERVO_ANGLE + (id - 1) * CONST.HTS_FILE.SERVO_SIZE];
            }
            this.servoTime = (UInt16)servoTimeMs;
            this.waitTime = (UInt16)waitTimeMs;
            return true;

        }

        public bool ReadFromCsv(string csv, int actionId, int poseId, byte maxServo)
        {
            string[] csvData = csv.Split(',');
            // Should have 2 * maxServo+ 8 fields
            // postId, enabled, servoTime, waitTime, {2 * maxServo} servos, headLed, mp3Folder, mp3File, mp3Vol
            if (csvData.Length != 2 * maxServo + 8) return false;
            // check if all numeric
            int[] data = new int[csvData.Length];
            for (int i = 0; i < csvData.Length; i++)
            {
                int value;
                if (!int.TryParse(csvData[i], out value))
                {
                    return false;
                }
                data[i] = value;
            }
            // if (poseId != data[0]) return false;
            this.actionId = (byte) actionId;
            this.poseId = (UInt16) poseId;
            this.enabled = (data[1] == 1);
            this.servoTime = (UInt16) data[2];
            this.waitTime = (UInt16) data[3];

            int idx = 4;
            int max_servo = Math.Min(maxServo, CONST.MAX_SERVO);

            for (int id = 1; id <= max_servo; id++)
            {
                servoAngle[id] = (byte) data[idx++];
            }
            idx = 4 + maxServo;
            for (int id = 1; id <= max_servo; id++)
            {
                servoLed[id] = (byte)data[idx++];
            }
            // 
            idx = 4 + 2 * maxServo;
            headLed = (byte) data[idx++];
            mp3Folder = (byte)data[idx++];
            mp3File = (byte)data[idx++];
            mp3Vol = (byte)data[idx++];

            return true;
        }

        public bool G2Conv(G2Map map)
        {
            for (byte id = 1; id <= 16; id++)
            {
                servoAngle[id] = map.Convert(id, servoAngle[id]);
            }
            // Special handle for Servo 17 ~ 20
            for (byte id = 17; id <= CONST.MAX_SERVO; id++)
            {
                servoAngle[id] = 255;
            }
            return true;
        }

        public byte[] GetData()
        {
            byte[] data = new byte[60];
            for (int i = 0; i < 60; i++) data[i] = 0;
            data[0] = 0xA9;
            data[1] = 0x9A;
            data[2] = CONST.CB.GET_ADPOSE.dataLen;  // GET.result len = SET.command len
            data[3] = CONST.CMD.UPD_ADPOSE;
            data[CONST.AI.POSE.OFFSET.ID] = actionId;
            data[CONST.AI.POSE.OFFSET.SEQ_HIGH] = (byte)((poseId >> 8) & 0xFF);
            data[CONST.AI.POSE.OFFSET.SEQ_LOW] = (byte)(poseId & 0xFF);

            data[CONST.AI.POSE.OFFSET.ENABLE] = (byte)(enabled ? 1 : 0);
            data[CONST.AI.POSE.OFFSET.EXECUTE_TIME] = (byte)(this.servoTime / 256);
            data[CONST.AI.POSE.OFFSET.EXECUTE_TIME + 1] = (byte)(this.servoTime % 256);
            data[CONST.AI.POSE.OFFSET.WAIT_TIME] = (byte)(this.waitTime / 256);
            data[CONST.AI.POSE.OFFSET.WAIT_TIME + 1] = (byte)(this.waitTime % 256);
            for (int id = 1; id <= CONST.MAX_SERVO; id++)
            {
                data[CONST.AI.POSE.OFFSET.ANGLE_ID + id] = servoAngle[id];
                if (this.servoLed[id] != 0)
                {
                    int h = (id - 1) / 4;
                    int l = 2 * (3 - ((id - 1) % 4));
                    data[CONST.AI.POSE.OFFSET.SERVO_LED + h] |= (byte)(this.servoLed[id] << l);
                }
            }
            data[CONST.AI.POSE.OFFSET.HEAD_LED] = this.headLed;
            data[CONST.AI.POSE.OFFSET.MP3_FOLDER] = this.mp3Folder;
            data[CONST.AI.POSE.OFFSET.MP3_FILE] = this.mp3File;
            data[CONST.AI.POSE.OFFSET.MP3_VOL] = this.mp3Vol;
            data[59] = 0xED;
            return data;
        }

        public static string GetCsvHeader()
        {
            StringBuilder sb = new StringBuilder();
            // sb.Append("序号, 启用, 动作时间, 帧时, ");
            sb.Append("Seq, Enabled, Servo time, Wait time, ");
            for (int id = 1; id <= CONST.MAX_SERVO; id++)
            {
                sb.Append(string.Format("{0}:Angle, ", id));
            }
            for (int id = 1; id <= CONST.MAX_SERVO; id++)
            {
                sb.Append(string.Format("{0}:LED, ", id));
            }
            sb.Append("Head LED, MP3 folder, MP3 file, MP3 volume\n");
            return sb.ToString();
        }


        public string GetCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("{0}, {1}, {2}, {3}, ", poseId, (enabled ? 1 : 0),  servoTime, waitTime));
            for (int id = 1; id <= CONST.MAX_SERVO; id++)
            {
                sb.Append(string.Format("{0}, ", servoAngle[id]));
            }
            for (int id = 1; id <= CONST.MAX_SERVO; id++)
            {
                sb.Append(string.Format("{0}, ", servoLed[id]));
            }
            sb.Append(string.Format("{0}, {1}, {2}, {3} \n", headLed, mp3Folder, mp3File, mp3Vol));
            return sb.ToString();
        }

        public bool ReadFromArray(byte[] data, int actionId, int poseId, int offset = 0)
        {
            // V2
            // For new version, single pose at a time
            enabled = (data[offset + CONST.AI.POSE.OFFSET.ENABLE] == 1);
            servoTime = (UInt16)(data[offset + CONST.AI.POSE.OFFSET.EXECUTE_TIME] * 256 + data[offset + CONST.AI.POSE.OFFSET.EXECUTE_TIME + 1]);
            waitTime = (UInt16)(data[offset + CONST.AI.POSE.OFFSET.WAIT_TIME] * 256 + data[offset + CONST.AI.POSE.OFFSET.WAIT_TIME + 1]);

            for (int id = 1; id <= CONST.MAX_SERVO; id++)
            {

                servoAngle[id] = data[offset + CONST.AI.POSE.OFFSET.ANGLE_ID + id];
                int h = (id - 1) / 4;
                int l = 2 * (3 - ((id - 1) % 4));
                servoLed[id] = (byte)((data[offset +CONST.AI.POSE.OFFSET.SERVO_LED + h] >> l) & 0b11);
            }
            headLed = data[offset + CONST.AI.POSE.OFFSET.HEAD_LED];
            mp3Folder = data[offset + CONST.AI.POSE.OFFSET.MP3_FOLDER];
            mp3File = data[offset + CONST.AI.POSE.OFFSET.MP3_FILE];
            mp3Vol = data[offset + CONST.AI.POSE.OFFSET.MP3_VOL];
            return true;
        }
    }
}
