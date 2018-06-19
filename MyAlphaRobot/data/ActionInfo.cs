using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class ActionInfo
    {
        public byte actionId;
        public string actionIdStr { get { return String.Format("{0:000}", actionId); } }
        public char actionCode { get; set; }
        public string actionName { get; set; }
        public bool isEmpty { get { return (poseCnt == 0); } }
        // public PoseInfo[] pose { get; set; }
        public PoseInfo[] pose;
        public int lastPose;
        public UInt16 poseCnt { get; set; }
        public long totalTime { get; set; }
        public List<byte> relatedServo = new List<byte>();
        public int relatedServoCnt { get { return (relatedServo == null ? 0 : relatedServo.Count); } }
        public string relatedServoStr { get; set; }
        public bool actionFileExists = false;
        public bool poseLoaded = true;
        public string actionLoaded
        {
            get
            {
                return (actionFileExists ? (poseLoaded ? "✔" : "✘") : "");
            }
        }

        public ActionInfo(byte actionId)
        {
            this.InitObject(actionId);
        }

        private void InitObject(byte actionId)
        {
            this.actionId = actionId;
            this.actionCode = (actionId < 26 ? (char)(((byte)'A') + actionId) : (char)(0x20));
            this.Reset();
        }

        public void Reset()
        {
            actionName = "";
            lastPose = 0;
            poseCnt = 0;
            totalTime = 0;
            relatedServo = new List<byte>();
            relatedServoStr = "";
            pose = new PoseInfo[] { };
            actionFileExists = false;
            poseLoaded = true;
        }

        // Stop criteria
        //   1) Enabled
        //   2) ExecuteTime = 0
        //   3) WaitTime = 0
        public void CheckPoses()
        {
            poseCnt = 0;
            totalTime = 0;
            relatedServo = new List<byte>();
            relatedServoStr = "";
            for (int poseId = 0; poseId < pose.Length; poseId++)
            {
                if ((pose[poseId].waitTime == 0) && (pose[poseId].servoTime == 0))
                {
                    break;
                }
                else
                {
                    poseCnt++;
                    // poseCnt must include those disabled entries, but no need to count for time & affected servo
                    if (pose[poseId].enabled)
                    {
                        totalTime += pose[poseId].waitTime;
                        for (byte id = 1; id <= CONST.MAX_SERVO; id++)
                        {
                            if (pose[poseId].servoAngle[id] <= CONST.SERVO.MAX_ANGLE)
                            {
                                if (!relatedServo.Exists(x => x == id))
                                {
                                    relatedServo.Add(id);
                                }
                            }
                        }

                    }
                }
            }
            if (relatedServo.Count > 0)
            {
                if (relatedServo.Count < CONST.MAX_SERVO)
                {
                    relatedServoStr = String.Join(",", relatedServo);
                }
                else
                {
                    relatedServoStr = "ALL Servos";
                }
            }
        }

        public bool ReadFromArray(byte[] data)
        {
            if (!ReadHeaderFromArray(data)) return false;
            SetPoseSize(poseCnt);

            for (int pId = 0; pId < poseCnt; pId++)
            {
                int offset = CONST.AI.HEADER.SIZE + pId * CONST.AI.POSE.SIZE;
                pose[pId].ReadFromArray(data, actionId, pId, offset);
            }
            CheckPoses();
            return true;
        }


        public bool ReadHeaderFromArray(byte[] data)
        {
            // V2 
            this.Reset();
            this.actionFileExists = true;
            this.poseLoaded = false;
            // this.actionId = data[CONST.AI.HEADER.OFFSET.ID];
            byte nameLen = data[CONST.AI.HEADER.OFFSET.NAME_LEN];
            if (nameLen > 20) nameLen = 20;
            byte[] bActionName = new byte[nameLen];
            for (int i = 0; i < nameLen; i++)
            {
                bActionName[i] = data[CONST.AI.HEADER.OFFSET.NAME + i];
            }
            this.actionName = Encoding.UTF8.GetString(bActionName);
            poseCnt = (UInt16)((data[CONST.AI.HEADER.OFFSET.POSE_CNT_HIGH] << 8) | data[CONST.AI.HEADER.OFFSET.POSE_CNT_LOW]);
            // totalTime = 

            long execute_time = 0;
            for (int i = 0; i < 4; i++)
            {
                execute_time = (execute_time << 8) | data[CONST.AI.HEADER.OFFSET.EXECUTE_TIME + i];
                /*
                execute_time *= 256;
                execute_time += data[CONST.AI.HEADER.OFFSET.EXECUTE_TIME + i];
                */
            }

            this.totalTime = execute_time;
            return true;
        }


        public bool ReadFromAESX(byte[] actionData, int actionId, string actionName)
        {
            if (this.actionId != actionId) return false;
            string fileId = "ubx-alpha";
            bool match = true;
            for (int i = 0; i < fileId.Length; i++)
            {
                if (actionData[4 + i] != fileId[i])
                {
                    match = false;
                    break;
                }
            }
            if (!match) return false;
            this.Reset();
            this.actionName = actionName;
            int poseCnt = (actionData.Length - CONST.AESX_FILE.HEADER_SIZE - CONST.AESX_FILE.TAIL_SIZE) / CONST.AESX_FILE.RECORD_SIZE;

            if (poseCnt > CONST.AI.MAX_POSES) poseCnt = CONST.AI.MAX_POSES; // set max pose/action

            SetPoseSize((UInt16)poseCnt);
            for (int poseId = 0; poseId < poseCnt; poseId++)
            {
                if (!pose[poseId].ReadFromAESX(actionData, this.actionId, poseId))
                {
                    this.Reset();
                    return false;
                }
            }

            CheckPoses();
            return true;
        }

        public bool ReadFromHTS(byte[] actionData, int actionId, string actionName)
        {
            if (this.actionId != actionId) return false;
            int remainByte = (actionData.Length - CONST.HTS_FILE.HEADER_SIZE - CONST.HTS_FILE.TAIL_SIZE) % CONST.HTS_FILE.RECORD_SIZE;
            if (remainByte > 0) return false;

            int poseCnt = (actionData.Length - CONST.HTS_FILE.HEADER_SIZE - CONST.HTS_FILE.TAIL_SIZE) / CONST.HTS_FILE.RECORD_SIZE;
            if (poseCnt > CONST.AI.MAX_POSES) poseCnt = CONST.AI.MAX_POSES; // set max pose/action

            this.Reset();
            this.actionName = actionName;

            SetPoseSize((UInt16)poseCnt);
            for (int poseId = 0; poseId < poseCnt; poseId++)
            {
                if (!pose[poseId].ReadFromHTS(actionData, this.actionId, poseId))
                {
                    this.Reset();
                    return false;
                }
            }
            CheckPoses();
            return true;
        }

        public byte[] GetData()
        {
            byte[] data = new byte[60];
            for (int i = 0; i < 60; i++) data[i] = 0;
            data[0] = 0xA9;
            data[1] = 0x9A;
            data[2] = 0x38;
            data[3] = CONST.CMD.UPD_ADHEADER;
            data[CONST.AI.HEADER.OFFSET.ID] = actionId;
            byte[] bActionName = Encoding.UTF8.GetBytes(actionName);
            int bCnt = bActionName.Length;
            for (int i = 0; i < 20; i++)
            {
                if (i < bCnt) data[CONST.AI.HEADER.OFFSET.NAME + i] = bActionName[i];
            }
            data[CONST.AI.HEADER.OFFSET.NAME_LEN] = (byte)(bCnt > 20 ? 20 : bCnt);
            data[CONST.AI.HEADER.OFFSET.POSE_CNT_HIGH] = (byte)((poseCnt >> 8) & 0xFF);
            data[CONST.AI.HEADER.OFFSET.POSE_CNT_LOW] = (byte)(poseCnt & 0xFF);

            long execute_time = this.totalTime;
            for (int i = 3; i >= 0; i--)
            {
                data[CONST.AI.HEADER.OFFSET.EXECUTE_TIME + i] = (byte)(execute_time % 256);
                execute_time /= 256;

            }
            foreach (byte b in relatedServo)
            {
                // b is id from 1 ~ 20, must be converted to 0 ~ 19 for merging data
                int h, l;
                h = (b - 1) / 8;
                l = (b - 1) % 8;
                data[CONST.AI.HEADER.OFFSET.AFFECT_SERVO + h] |= (byte)(1 << l);
            }
            data[59] = 0xED;
            return data;
        }

        public byte[] GetPoseData(UInt16 poseId)
        {
            return pose[poseId].GetData();
        }

        public void SetPoseSize(UInt16 size)
        {
            Array.Resize(ref pose, size);
            for (UInt16 pId = 0; pId < pose.Length; pId++)
            {
                pose[pId] = new PoseInfo(actionId, pId);
            }
        }

        public UInt16 InsertPose(UInt16 poseId)
        {
            UInt16 size = (UInt16)pose.Length;
            Array.Resize(ref pose, size + 1);
            for (UInt16 i = size; i > poseId; i--)
            {
                pose[i] = pose[i - 1];
                pose[i].poseId = i;
            }
            pose[poseId] = new PoseInfo(actionId, poseId);
            return poseId;
        }

        public int DeletePose(UInt16 poseId)
        {
            if ((poseId < 0) || (poseId >= pose.Length)) return -1;
            for (UInt16 i = poseId; i < pose.Length - 1; i++)
            {
                pose[i] = pose[i + 1];
                pose[i].poseId = i;
            }
            pose[pose.Length - 1] = null;
            Array.Resize(ref pose, pose.Length - 1);
            if (poseId > pose.Length - 1) poseId = (UInt16)(pose.Length - 1);
            return poseId;
        }


    }
}
