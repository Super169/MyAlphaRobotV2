using MyUtil;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyAlphaRobot
{

    public partial class UBTController
    {

        public byte V2_CheckSum(byte[] command)
        {
            byte len = command[2];
            int sum = 0;
            for (int i = 0; i < len; i++)
            {
                sum += command[2 + i];
            }
            return (byte)(sum % 256);
        }


        private bool SendRobotCommand(byte[] cmd, CONST.CB.CMD_INFO info)
        {
            // Default as standard UBT command with 10 bytes return
            if (info.minBytes < 0)
            {
                throw new Exception(string.Format("Command {0:X2}, expectCnt not configed", info.code));
            }
            return SendRobotCommand(cmd, (uint) info.minBytes, (info.maxMs <= 0 ? DEFAULT_COMMAND_TIMEOUT : info.maxMs));
        }

        // Some operation has variable result (e.g. lock servo which can lock / unlock multiple servo
        private bool SendRobotCommand(byte[] cmd, CONST.CB.CMD_INFO info, uint expectCnt)
        {
            return SendRobotCommand(cmd, expectCnt, (info.maxMs <= 0 ? DEFAULT_COMMAND_TIMEOUT : info.maxMs));
        }


        private bool SendRobotCommand(byte[] cmd, uint expectCnt, long maxMs = DEFAULT_COMMAND_TIMEOUT)
        {
            if (!robot.isConnected) return false;

            if (cmd.Length < 6) return false;
            byte len = cmd[2];
            if (cmd.Length != len + 4)
            {
                Array.Resize<byte>(ref cmd, len + 4);
            }
            cmd[0] = 0xA9;
            cmd[1] = 0x9A;
            cmd[len + 2] = V2_CheckSum(cmd);
            cmd[len + 3] = 0xED;

            robot.ClearRxBuffer();

            // public bool SendCommand(byte[] command, int count, long expBytes, long maxMs = -1)
            bool success = robot.SendCommand(cmd, cmd.Length, expectCnt, maxMs);
            
            // To minimize program change, put robot.buffer to receiveBuffer
            receiveBuffer.Clear();
            if (robot.Available > 0)
            {
                receiveBuffer.AddRange(robot.ReadAll().ToList<byte>());
            }
            return success;
        }

        // A9 9A 02 11 13 ED
        public bool V2_RefreshAngle()
        {
            byte[] command = { 0xA9, 0x9A, 2, CONST.CMD.SERVOANGLE, 0x00, 0xED };
            //if (!V2_SendCommand(command))
            if (!SendRobotCommand(command, CONST.CB.SERVOANGLE))
            {
                UpdateInfo("Fail getting servo angles", MyUtil.UTIL.InfoType.error);
                return false;
            }
            for (byte id = 1; id <= CONST.MAX_SERVO; id++)
            {
                int pos = 4 + 2 * (id - 1);
                byte angle = receiveBuffer[pos];
                if (angle > 0xF0)
                {
                    servo[id].Update(false, 0xff, false);
                }
                else
                {
                    bool locked = (receiveBuffer[pos + 1] == 1);
                    servo[id].Update(true, angle, locked);
                }
                RefreshServo(id);
            }

            return true;
        }

        // A9 9A 03 12 {id} {sum} ED
        public byte V2_GetAngle(byte id)
        {
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.ONEANGLE, id, 00, 0xED };

            // if (!V2_SendCommand(command) ||
            if (!SendRobotCommand(command, CONST.CB.ONEANGLE) ||
                (receiveBuffer.Count != 9) || (receiveBuffer[2] != 5) ||
                (receiveBuffer[3] != CONST.CMD.ONEANGLE) || (receiveBuffer[4] != id))
            {
                UpdateInfo(String.Format("Fail getting angle"), MyUtil.UTIL.InfoType.error);
                return 0xFF;
            }
            return receiveBuffer[7];
        }


        // A9 9A {len} 21 ({id}) {sum} ED
        // A9 9A {len} 22 ({id}) {sum} ED
        public List<data.ServoInfo> V2_LockServo(List<data.ServoInfo> lsi, bool goLock)
        {
            byte[] command = new byte[lsi.Count + 6];
            command[2] = (byte)(lsi.Count + 2);
            command[3] = (goLock ? CONST.CMD.LOCKSERVO : CONST.CMD.UNLOCKSERVO);
            for (int i = 0; i < lsi.Count; i++)
            {
                command[4 + i] = lsi[i].id;
            }
            // if (!V2_SendCommand(command))
            uint expectCnt = (uint) (7 + 2 * lsi.Count); 
            if (!SendRobotCommand(command, (goLock ? CONST.CB.LOCKSERVO : CONST.CB.UNLOCKSERVO), expectCnt))
            {
                UpdateInfo(String.Format("Fail to {0}lock servo", (goLock ? "" : "un")), MyUtil.UTIL.InfoType.error);
                return null;
            }
            List<data.ServoInfo> result = new List<data.ServoInfo>();
            byte cnt = receiveBuffer[4];
            for (int i = 0; i < cnt; i++)
            {
                int pos = 5 + 2 * i;
                byte id = receiveBuffer[pos];
                byte angle = receiveBuffer[pos + 1];
                result.Add(new data.ServoInfo(id, angle));
                servo[id].Update(true, angle, goLock);
                RefreshServo(id);
            }
            return result;
        }

        // A9 9A {len} 23 ({id} {angle} {time}) {sum} ED
        public bool V2_MoveServo(byte id, byte angle, UInt16 time)
        {
            byte[] command = { 0xA9, 0x9A, 0x06, CONST.CMD.SERVOMOVE, id, angle, (byte) (time >> 8), (byte) (time), 00, 0xED };
            //if (!V2_SendCommand(command) || (receiveBuffer[4] != 1))
            uint expectCnt = 11; // Fix to single servoe, 7 + 4 * 1 = 11
            if (!SendRobotCommand(command, CONST.CB.SERVOMOVE, expectCnt) || (receiveBuffer[4] != 1))
            {
                UpdateInfo(String.Format("Fail moveing servo"), MyUtil.UTIL.InfoType.error);
                return false;
            }
            servo[id].Update(true, angle, true);
            RefreshServo(id);
            UpdateInfo(String.Format("Turn servo {0} to {1} degree", id, angle));
            return true;
        }

        // A9 9A 03 14 {id} {sum} ED
        public UInt16 V2_GetAdjAngle(byte id)
        {
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.ONEADJANGLE, id, 00, 0xED };
            // if (!V2_SendCommand(command) ||
            if (!SendRobotCommand(command, CONST.CB.ONEADJANGLE) ||
                (receiveBuffer.Count != 9) || (receiveBuffer[2] != 5) ||
                (receiveBuffer[3] != CONST.CMD.ONEADJANGLE) || (receiveBuffer[4] != id))
            {
                UpdateInfo(String.Format("Fail getting adjustment"), MyUtil.UTIL.InfoType.error);
                return 0x7F7F;
            }
            UInt16 adjAngle = (UInt16)((receiveBuffer[5] << 8) | receiveBuffer[6]);
            return adjAngle;
        }

        // A9 9A 05 15 {id} {high} {low} {sum} ED
        public bool V2_SetAdjAngle(byte id, UInt16 adjAngle)
        {
            byte adjH, adjL;
            adjH = (byte)(adjAngle / 256);
            adjL = (byte)(adjAngle & 0xFF);
            byte[] command = { 0xA9, 0x9A, 0x05, CONST.CMD.SETADJANGLE, id, adjH, adjL, 00, 0xED };
            // if (!V2_SendCommand(command) ||
            if (!SendRobotCommand(command, CONST.CB.SETADJANGLE) ||
                (receiveBuffer.Count != 7) || (receiveBuffer[2] != 3) ||
                (receiveBuffer[3] != CONST.CMD.SETADJANGLE) || (receiveBuffer[4] != 0x00))
            {
                UpdateInfo(String.Format("Fail setting ajustment"), MyUtil.UTIL.InfoType.error);
                return false;
            }
            return true;
        }


        // A9 9A 04 16 {id} {mode} {sum} ED
        public bool V2_ServoCommand(byte id, byte mode)
        {
            UpdateInfo();
            // 0 ~ reset servo, 1 ~ Set 1500 position, 2 ~ Set lower boundary, 3 ~ Set upper boundary
            byte[] command = { 0xA9, 0x9A, 0x04, CONST.CMD.SERVOCOMMAND, id, mode, 00, 0xED };
            // if (!V2_SendCommand(command, true, 2000) ||
            if (!SendRobotCommand(command, CONST.CB.SERVOCMD) ||
                (receiveBuffer.Count != 7) || (receiveBuffer[2] != 3) ||
                 (receiveBuffer[3] != CONST.CMD.SERVOCOMMAND) || (receiveBuffer[4] != mode))
            {
                UpdateInfo(String.Format("执行舵机指令 {0} 失败", mode), MyUtil.UTIL.InfoType.error);
                return false;
            }
            return true;
        }


        // A9 9A ?? 23 <id,angle,time>* C8 ED 
        public void V2_SetPose(data.PoseInfo pi)
        {
            List<byte> movement = new List<byte>();
            int moveCnt = 0;
            for (byte id = 1; id <= CONST.MAX_SERVO; id++)
            {
                if (pi.servoAngle[id] <= CONST.SERVO.MAX_ANGLE)
                {
                    movement.Add(id);
                    movement.Add(pi.servoAngle[id]);
                    // movement.Add((byte)(pi.servoTime / CONST.SERVO_TIME_FACTOR));
                    UInt16 moveTime = pi.servoTime;
                    movement.Add((byte)(moveTime >> 8));
                    movement.Add((byte)(moveTime & 0xFF));
                    moveCnt++;
                }
            }
            if (moveCnt > 0)
            {
                int moveIdx = 0;
                while (moveIdx < moveCnt)
                {
                    int servoCnt = moveCnt - moveIdx;
                    if (servoCnt > 30) servoCnt = 30;
                    List<byte> command = new List<byte>();
                    command.Add(0xA9);
                    command.Add(0x9A);
                    command.Add(0x00);
                    command.Add(CONST.CMD.SERVOMOVE);
                    command.AddRange(movement.GetRange(4 * moveIdx, 4 * servoCnt));
                    command.Add(0);
                    command.Add(0xED);

                    byte[] data = command.ToArray();
                    data[2] = (byte)((4 * servoCnt) + 2);
                    // V2_SendCommand(data);
                    uint expectCnt = (uint) (7 + 4 * servoCnt);  
                    SendRobotCommand(data, CONST.CB.SERVOMOVE, expectCnt);

                    moveIdx += servoCnt;

                }
            }
        }


        // A9 9A 04 24 01 00 29 ED
        public bool V2_SetLED(byte id, byte mode)
        {
            if (mode > 0) mode = 1;
            byte[] command = { 0xA9, 0x9A, 0x04, CONST.CMD.LED, id, mode, 00, 0xED };
            // V2_SendCommand(command);
            SendRobotCommand(command, CONST.CB.LED);
            return true;
        }

        // A9 9A 04 24 01 00 29 ED
        public bool V2_SetHeadLED(byte mode)
        {
            if (mode > 0) mode = 1;
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.HEAD_LED, mode, 00, 0xED };
            //V2_SendCommand(command);
            SendRobotCommand(command, CONST.CB.SET_HEADLED);
            return true;
        }

        // A9 9A 04 36 00 0F 49 ED
        public bool V2_MP3SetVol(byte playVol)
        {
            byte[] command = { 0xA9, 0x9A, 0x04, CONST.CMD.MP3.SetVol, 00, playVol, 0xED };
            // V2_SendCommand(command);
            SendRobotCommand(command, CONST.CB.MP3_SETVOLUME);
            return true;
        }

        // A9 9A 04 33 00 01 38 ED
        public bool V2_MP3PlayFile(byte folderSeq, byte fileSeq)
        {
            byte[] command = { 0xA9, 0x9A, 0x04, CONST.CMD.MP3.PlayFile, folderSeq, fileSeq, 00, 0xED };
            // V2_SendCommand(command);
            SendRobotCommand(command, CONST.CB.MP3_PLAYFILE);
            return true;
        }

        // A9 9A 02 32 34 ED
        public bool V2_MP3Stop()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.MP3.Stop, 00, 0xED };
            // V2_SendCommand(command);
            SendRobotCommand(command, CONST.CB.MP3_STOP);
            return true;
        }

        // A9 9A 05 18 {id} {angle} {minor} {sum} ED
        public bool V2_SetAngle(byte id, byte angle, byte minor)
        {
            byte[] command = { 0xA9, 0x9A, 0x05, CONST.CMD.SET_ANGLE, id, angle, minor, 00, 0xED };
            // if (!V2_SendCommand(command, true, 2000) ||
            if (!SendRobotCommand(command, CONST.CB.SETANGLE) ||
                (receiveBuffer.Count != 7) || (receiveBuffer[2] != 3) ||
                 (receiveBuffer[3] != CONST.CMD.SET_ANGLE))
            {
                UpdateInfo("执行舵机指令失败", MyUtil.UTIL.InfoType.error);
                return false;
            }

            if (receiveBuffer[4] != 0)
            {
                switch (receiveBuffer[4])
                {
                    case 3:
                        UpdateInfo("当前舵机模式不能进行舵机修正", MyUtil.UTIL.InfoType.error);
                        return false;
                    case 5:
                        UpdateInfo("修正角度跟舵机预设角度相差太大", MyUtil.UTIL.InfoType.error);
                        return false;
                }
                return false;
            }
            return true;
        }

        public byte[] V2_GetEventHeader(byte mode)
        {
            CONST.CB.CMD_INFO info = CONST.CB.GET_EVENT_HEADER;
            byte[] command = { 0xA9, 0x9A, 0x03, info.code, mode, 00, 0xED };
            if (!SendRobotCommand(command, info) ||
                (receiveBuffer.Count != info.minBytes) || (receiveBuffer[2] != info.dataLen) ||
                (receiveBuffer[3] != info.code) || 
                (receiveBuffer[CONST.EH.OFFSET.MODE] != mode))
            {
                UpdateInfo(String.Format("Fail getting event header"), MyUtil.UTIL.InfoType.error);
                return null;
            }
            return receiveBuffer.ToArray();
        }

        public byte[] V2_GetEventData(byte mode, byte startIdx)
        {
            CONST.CB.CMD_INFO info = CONST.CB.GET_EVENT_DATA;
            byte[] command = { 0xA9, 0x9A, 0x04, info.code, mode, startIdx, 00, 0xED };
            if (!SendRobotCommand(command, info) ||
                (receiveBuffer.Count != info.minBytes) || (receiveBuffer[2] != info.dataLen) ||
                (receiveBuffer[CONST.ED.OFFSET.MODE] != mode) ||
                (receiveBuffer[CONST.ED.OFFSET.START_IDX] != startIdx))
            {
                UpdateInfo(String.Format("Fail getting event header"), MyUtil.UTIL.InfoType.error);
                return null;
            }
            return receiveBuffer.ToArray();
        }

        public bool V2_SaveEventHeader(byte mode, byte count, byte action)
        {
            CONST.CB.CMD_INFO info = CONST.CB.SAVE_EVENT_HEADER;
            byte[] command = { 0xA9, 0x9A, 0x0C, info.code, mode, RobotHandler.version, count, action, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xED };
            if (!V2_SendCommandWithSingleReturn(command))
            {
                UpdateInfo(String.Format("Fail saving event header"), MyUtil.UTIL.InfoType.error);
                return false;
            }
            return true;
        }

        public bool V2_SaveEventData(byte mode, byte startIdx, byte count, byte[] data)
        {
            if (count > 8) return false;
            int startPos = startIdx * BLOCKLY.EVENT.SIZE;
            int size = count * BLOCKLY.EVENT.SIZE;
            if (startPos > data.Length) return false;
            if (startPos + size > data.Length) return false;

            CONST.CB.CMD_INFO info = CONST.CB.SAVE_EVENT_DATA;
            byte[] command = new byte[128];
            for (int i = 0; i < 128; i++) command[i] = 0;
            command[2] = 124;
            command[3] = info.code;
            command[4] = mode;
            command[5] = startIdx;
            command[6] = count;
            for (int i = 0; i < size; i++)
            {
                command[16 + i] = data[startPos + i];
            }
            if (!V2_SendCommandWithSingleReturn(command))
            {
                UpdateInfo(String.Format("Fail saving event data"), MyUtil.UTIL.InfoType.error);
                return false;
            }
            return true;
        }

    }
}