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

        public bool V2_WaitResult(byte cmd, bool expResult, long maxMs)
        {
            System.Threading.Thread.Sleep(1); // always wait 1ms for result first
            if (!expResult)
            {
                return true;
            }

            if (maxMs < 1) maxMs = 1;
            if (maxMs > MAX_WAIT_MS) maxMs = MAX_WAIT_MS;

            long startTicks = DateTime.Now.Ticks;
            long endTicks = startTicks + maxMs * TimeSpan.TicksPerMillisecond;
            long lastBufferCount = 0;
            bool resultDetected = false;

            while (!resultDetected)
            {
                if (receiveBuffer.Count >= 6)
                {
                    for (int i = 0; i <= receiveBuffer.Count - 6; i++)
                    {
                        // start code ready, command code matched
                        if ((receiveBuffer[i] == 0xA9) && (receiveBuffer[i + 1] == 0x9A) &&
                            (receiveBuffer[i + 3] == cmd))
                        {
                            int len = receiveBuffer[i + 2];
                            if (len == 0)
                            {
                                // Special case for long return
                                len = receiveBuffer[i + 4] * 256 + receiveBuffer[i + 5];
                            }
                            if (receiveBuffer.Count >= i + len + 4)
                            {
                                // end code matched
                                if (receiveBuffer[i + len + 3] == 0xED)
                                {
                                    resultDetected = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!resultDetected)
                {
                    // Timeout and no new data in 1 ms
                    if ((DateTime.Now.Ticks > endTicks) && (receiveBuffer.Count == lastBufferCount))
                    {
                        break;
                    }
                    lastBufferCount = receiveBuffer.Count;
                    System.Threading.Thread.Sleep(1); // always 1ms for result
                }
            }

            return resultDetected;
        }

        // A9 9A {len} {cmd} [{parm}] {sum} ED
        public bool V2_SendCommand(byte[] command, bool expResult  =true, long maxMs = DEFAULT_COMMAND_TIMEOUT)
        {
            if (!serialPort.IsOpen) return false;
            if (command.Length < 6) return false;
            byte len = command[2];
            if (command.Length != len + 4)
            {
                Array.Resize<byte>(ref command, len + 4);
            }
            command[0] = 0xA9;
            command[1] = 0x9A;
            command[len + 2] = V2_CheckSum(command);
            command[len + 3] = 0xED;
            bool CheckBattery = (command[2] == CONST.CMD.CHECKBATTERY);

            ClearSerialBuffer();
            serialPort.Write(command, 0, len + 4);
            bool result = V2_WaitResult(command[3], expResult, maxMs);
            return result;
        }

        // A9 9A 02 11 13 ED
        public bool V2_RefreshAngle()
        {
            byte[] command = { 0xA9, 0x9A, 2, CONST.CMD.SERVOANGLE, 0x00, 0xED };
            if (!V2_SendCommand(command))
            {
                UpdateInfo("Fail getting servo angles", UTIL.InfoType.error);
                return false;
            }
            for (byte id = 1; id <= CONST.MAX_SERVO; id++)
            {
                int pos = 4 + 2 * (id - 1);
                byte angle = receiveBuffer[pos];
                if (angle > 0xF0)
                {
                    servo[id].Update(false, 0xff, false);
                } else
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
            if (!V2_SendCommand(command) ||
                (receiveBuffer.Count != 9) || (receiveBuffer[2] != 5) ||
                (receiveBuffer[3] != CONST.CMD.ONEANGLE) || (receiveBuffer[4] != id))
            {
                UpdateInfo(String.Format("Fail getting angle"), UTIL.InfoType.error);
                return 0xFF;
            }
            return receiveBuffer[5];
        }


        // A9 9A {len} 21 ({id}) {sum} ED
        // A9 9A {len} 22 ({id}) {sum} ED
        public List<data.ServoInfo> V2_LockServo(List<data.ServoInfo> lsi, bool goLock)
        {
            byte[] command = new byte[lsi.Count + 6];
            command[2] = (byte) (lsi.Count + 2);
            command[3] = (goLock ? CONST.CMD.LOCKSERVO : CONST.CMD.UNLOCKSERVO);
            for (int i = 0; i < lsi.Count; i++)
            {
                command[4 + i] = lsi[i].id;
            }
            if (!V2_SendCommand(command))
            {
                UpdateInfo(String.Format("Fail to {0}lock servo", (goLock ? "" : "un")), UTIL.InfoType.error);
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
        public bool V2_MoveServo(byte id, byte angle, byte time)
        {
            byte[] command = { 0xA9, 0x9A, 0x05, CONST.CMD.SERVOMOVE, id, angle, time, 00, 0xED };
            if (!V2_SendCommand(command) || (receiveBuffer[4] != 1))
            {
                UpdateInfo(String.Format("Fail moveing servo"), UTIL.InfoType.error);
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
            if (!V2_SendCommand(command) || 
                (receiveBuffer.Count != 9) || (receiveBuffer[2] != 5) || 
                (receiveBuffer[3] != CONST.CMD.ONEADJANGLE) || (receiveBuffer[4] != id))
            {
                UpdateInfo(String.Format("Fail getting adjustment"), UTIL.InfoType.error);
                return 0x7F7F;
            }
            UInt16 adjAngle = (UInt16)((receiveBuffer[5] << 8) | receiveBuffer[6]);
            return adjAngle;
        }

        // A9 9A 03 15 {id} {high} {low} {sum} ED
        public bool V2_SetAdjAngle(byte id, UInt16 adjAngle)
        {
            byte adjH, adjL;
            adjH = (byte) (adjAngle / 256);
            adjL = (byte)(adjAngle & 0xFF);
            byte[] command = { 0xA9, 0x9A, 0x05, CONST.CMD.SETADJANGLE, id, adjH, adjL, 00, 0xED };
            if (!V2_SendCommand(command) ||
                (receiveBuffer.Count != 7) || (receiveBuffer[2] != 3) ||
                (receiveBuffer[3] != CONST.CMD.SETADJANGLE) || (receiveBuffer[4] != 0x00))
            {
                UpdateInfo(String.Format("Fail setting ajustment"), UTIL.InfoType.error);
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
                    movement.Add((byte) (pi.servoTime / CONST.SERVO_TIME_FACTOR));
                    moveCnt++;
                }
            }
            if (moveCnt > 0)
            {
                int moveIdx = 0;
                while (moveIdx < moveCnt) {
                    int servoCnt = moveCnt - moveIdx;
                    if (servoCnt > 19) servoCnt = 19;
                    List<byte> command = new List<byte>();
                    command.Add(0xA9);
                    command.Add(0x9A);
                    command.Add(0x00);
                    command.Add(CONST.CMD.SERVOMOVE);
                    command.AddRange(movement.GetRange(3 * moveIdx, 3 * servoCnt));
                    command.Add(0);
                    command.Add(0xED);

                    byte[] data = command.ToArray();
                    data[2] = (byte)((3 * servoCnt) + 2);
                    V2_SendCommand(data);

                    moveIdx += servoCnt;

                }
            }
        }


        // A9 9A 04 24 01 00 29 ED
        public bool V2_SetLED(byte id, byte mode)
        {
            if (mode > 0) mode = 1;
            byte[] command = { 0xA9, 0x9A, 0x04, CONST.CMD.LED, id, mode, 00, 0xED };
            V2_SendCommand(command);
            return true;
        }

        // A9 9A 04 24 01 00 29 ED
        public bool V2_SetHeadLED(byte mode)
        {
            if (mode > 0) mode = 1;
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.HEAD_LED, mode, 00, 0xED };
            V2_SendCommand(command);
            return true;
        }

        // A9 9A 04 36 00 0F 49 ED
        public bool V2_MP3SetVol(byte playVol)
        {
            byte[] command = { 0xA9, 0x9A, 0x04, CONST.CMD.MP3.SetVol, 00, playVol, 0xED };
            V2_SendCommand(command);
            return true;
        }

        // A9 9A 04 33 00 01 38 ED
        public bool V2_MP3PlayFile(byte folderSeq, byte fileSeq)
        {
            byte[] command = { 0xA9, 0x9A, 0x04, CONST.CMD.MP3.PlayFile, folderSeq, fileSeq, 00, 0xED };
            V2_SendCommand(command);
            return true;
        }

        // A9 9A 02 32 34 ED
        public bool V2_MP3Stop()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.MP3.Stop, 00, 0xED };
            V2_SendCommand(command);
            return true;
        }

    }
}