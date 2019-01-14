using MyUtil;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MyAlphaRobot
{
    public partial class UBTController
    {
        private RobotConnection robot;

        private const long DEFAULT_COMMAND_TIMEOUT = 500;   // Default 500ms, according to spec, servo return at 400ms, add some overhead for ESP8266 handler
        private const long MAX_WAIT_MS = 10000;             // Default 10s, for any command, it should not wait more than 10s
        private const int DEFAULT_TRY_COUNT = 3;

        // private SerialPort serialPort = new SerialPort();

        private List<byte> receiveBuffer = new List<byte>();
        public delegate void DelegateUpdateInfo(string msg, MyUtil.UTIL.InfoType iType);
        private DelegateUpdateInfo updateInfo;
        public delegate void DelegateRefreshServo(byte id);
        private DelegateRefreshServo refreshServo;
        data.ServoInfo[] servo = new data.ServoInfo[CONST.MAX_SERVO + 1];              // For better match id, Servo[1] : id = 1, so it need server[1] ~ servo[16], ignore Servo[0];

        public bool connected { get { return robot.isConnected; } }

        public data.RobotInfo robotInfo = new data.RobotInfo();
        public data.BoardConfig config;
        public data.NetworkConfig networkConfig;
        public data.ComboTable comboTable;
        public data.ActionTable actionTable;
        public bool actionTableReady = false;
        public bool mpuExists = false;
        public bool eventHandlerEnabled = true;

        public UBTController(DelegateUpdateInfo fxUpdateInfo, DelegateRefreshServo fxRefreshServo, RobotConnection robot)
        {
            updateInfo = fxUpdateInfo;
            refreshServo = fxRefreshServo;
            this.robot = robot;

            InitServo();
            // InitSerialPort();
            config = new data.BoardConfig();
            networkConfig = new data.NetworkConfig();
            comboTable = new data.ComboTable();
            actionTable = new data.ActionTable();
        }

        private void InitServo()
        {
            for (byte i = 1; i <= CONST.MAX_SERVO; i++)
            {
                servo[i] = new data.ServoInfo(i);
            }
        }

        /*
        private void InitSerialPort()
        {
            serialPort.DataReceived += SerialPort_DataReceived;
        }
        */

        private void UpdateInfo(String msg = "", MyUtil.UTIL.InfoType iType = MyUtil.UTIL.InfoType.message)
        {
            if (updateInfo != null)
            {
                updateInfo(msg, iType);
            }
        }

        private void RefreshServo(byte id)
        {
            if (refreshServo != null)
            {
                refreshServo(id);
            }
        }

        public data.ServoInfo GetServo(byte id)
        {
            return servo[id];
        }

        public bool IsExists(byte id)
        {
            return servo[id].exists;
        }

        public bool Isocked(byte id)
        {
            return servo[id].locked;
        }

        public byte GetAngle(byte id)
        {
            return servo[id].angle;
        }

        public void PostConnection()
        {
            ExitUSBTTL();
            GetRobotInfo();
            CheckMpu();
            GetConfig();
            GetNetworkConfig();
            DownloadActionTable();
            RefreshAngle();
            CheckEventHandler();
        }

        public void PostDisconnect()
        {
            ResetActionTable();
            ResetAllServo();
        }

        private void ExitUSBTTL()
        {
            byte[] command = { 0xA9, 0x9A, 0x01, 0x06, 0x09 };
            bool success = robot.SendCommand(command, command.Length, 0, 0);
            robot.ClearRxBuffer();
            receiveBuffer.Clear();
        }

 
        public void ResetAllServo()
        {
            for (byte id = 1; id <= CONST.MAX_SERVO; id++)
            {
                if (servo[id].exists)
                {
                    servo[id].Update(false, 0xff, false);
                    RefreshServo(id);
                }
            }
        }

        private void ClearSerialBuffer()
        {
            receiveBuffer.Clear();
        }

        #region Refresh Angle
        /*
         * Refresh Angle 
         * 
         * 
         */

        public bool RefreshAngle()
        {
            //if (!serialPort.IsOpen) return false;
            if (!robot.isConnected) return false;
            return V2_RefreshAngle();
        }

        #endregion

        #region Lock Servo
        /*
         * Lock Servo
         * 
         * 
         */
        public List<data.ServoInfo> LockServo(byte id, bool goLock)
        {
            // if (!serialPort.IsOpen) return null;
            if (id > CONST.MAX_SERVO) return null;
            List<data.ServoInfo> lsi = new List<data.ServoInfo>();
            lsi.Add(new data.ServoInfo(id));
            return LockServo(lsi, goLock);
        }

        public List<data.ServoInfo> LockServo(List<data.ServoInfo> lsi, bool goLock)
        {
            // if (!serialPort.IsOpen) return null;
            if ((lsi == null) || (lsi.Count() == 0)) return null;
            return V2_LockServo(lsi, goLock);
        }


        #endregion

        #region Move Servo
        /*
         * Lock Servo
         * 
         * 
         */

        public bool MoveServo(byte id, byte angle, UInt16 time)
        {
            // if (!serialPort.IsOpen) return false;
            return V2_MoveServo(id, angle, time);
        }

        #endregion


        public String GetVersion(bool forceCheck = true)
        {
            if ((!RCVersion.ready) || (forceCheck))
            {
                byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.GET_VERSION, 0, 0xED };
                // if (!V2_SendCommand(command, true)) return "";
                if (!SendRobotCommand(command, CONST.CB.GET_VERSION)) return "";
                if (receiveBuffer[2] != CONST.CB.GET_VERSION.dataLen)
                {
                    return "";
                }
                RCVersion.major = receiveBuffer[4];
                RCVersion.minor = receiveBuffer[5];
                RCVersion.build = receiveBuffer[6];
                RCVersion.fix = receiveBuffer[7];
                RCVersion.ready = true;
            }
            return RCVersion.ToString();
            /*
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.GET_VERSION, 0, 0xED };
            // if (!V2_SendCommand(command, true)) return "";
            if (!SendRobotCommand(command, CONST.CB.GET_VERSION)) return "";
            if (receiveBuffer[2] != CONST.CB.GET_VERSION.dataLen)
            {
                return "";
            }
            string version = string.Format("{0}.{1}.{2}", receiveBuffer[4], receiveBuffer[5], receiveBuffer[6]);
            if (receiveBuffer[7] > 0) version += string.Format("  [Fix {0}]", receiveBuffer[7]);
            return version;
            */
        }

        public bool GetRobotInfo()
        {
            robotInfo.version = GetVersion();
            return true;
        }

        public bool GetConfig()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.GET_CONFIG, 0, 0xED };
            // if (!V2_SendCommand(command, true)) return false;
            if (!SendRobotCommand(command, CONST.CB.GETCONFIG)) return false;
            if (receiveBuffer[2] != CONST.CB.GETCONFIG.dataLen)
            {
                return false;
            }
            config.ReadFromArray(receiveBuffer.ToArray());
            return true;
        }

        public bool SetConfig()
        {
            config.data[0] = 0xA9;
            config.data[1] = 0x9A;
            config.data[2] = CONST.CB.GETCONFIG.dataLen;  // // GET.result len = SET.command len
            config.data[3] = CONST.CMD.SET_CONFIG;
            // takes long time in writing SPIFFS
            return V2_TryCommand(config.data, 1000);
        }

        public bool GetNetworkConfig()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.GET_NETWORK_CONFIG, 0, 0xED };
            // if (!V2_SendCommand(command, true)) return false;
            if (!SendRobotCommand(command, CONST.CB.GET_WIFI_CONFIG)) return false;
            if (receiveBuffer[2] != CONST.CB.GET_WIFI_CONFIG.dataLen)
            {
                return false;
            }
            networkConfig.ReadFromArray(receiveBuffer.ToArray());
            String ssid = networkConfig.apName;
            return true;
        }

        public bool SetNetworkConfig()
        {
            networkConfig.data[0] = 0xA9;
            networkConfig.data[1] = 0x9A;
            networkConfig.data[2] = CONST.CB.GET_WIFI_CONFIG.dataLen;  // GET.result len = SET.command len
            networkConfig.data[3] = CONST.CMD.SET_NETWORK_CONFIG;
            // takes long time in writing SPIFFS
            return V2_TryCommand(networkConfig.data, 1000);
        }

        public bool DefaultConfig()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.DEFAULT_CONFIG, 0, 0xED };
            return (V2_SendCommandWithSingleReturn(command));
        }

        public void ResetComboTable()
        {
            comboTable.Reset();
        }

        public bool DownloadComboTable()
        {
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.GET_COMBO, 0, 0, 0xED };
            for (byte i = 0; i < CONST.CI.MAX_COMBO; i++)
            {
                command[4] = i;
                //if (V2_SendCommand(command, true, 1000))
                if (SendRobotCommand(command, CONST.CB.GET_COMBO))
                {
                    if (receiveBuffer[2] == CONST.CB.GET_COMBO.dataLen)
                    {
                        comboTable.ReadFromArray(i, receiveBuffer.ToArray());
                    }
                }
            }
            return true;
        }

        public void ResetActionTable()
        {
            for (byte actionId = 0; actionId < CONST.AI.MAX_ACTION; actionId++)
            {
                actionTable.action[actionId].Reset();
            }
        }

        public bool DownloadActionTable()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.GET_ADLIST, 0, 0xED };
            //if (V2_SendCommand(command, true))
            if (SendRobotCommand(command, CONST.CB.GET_ADLIST))
            {
                byte len = receiveBuffer[2];
                if (len == CONST.CB.GET_ADLIST.dataLen)
                {
                    byte flagLen = (CONST.AI.MAX_ACTION + 7) / 8;
                    byte[] adFlag = new byte[flagLen];
                    for (int i = 0; i < flagLen; i++)
                    {
                        adFlag[i] = receiveBuffer[i + 4];
                    }
                    for (byte actionId = 0; actionId < CONST.AI.MAX_ACTION; actionId++)
                    {
                        int h = actionId / 8;
                        int l = 7 - (actionId % 8);
                        byte flag = (byte)(adFlag[h] & (1 << l));
                        actionTable.action[actionId].actionFileExists = (flag != 0);
                        if (flag != 0)
                        {
                            try
                            {
                                DownloadAction(actionId, false);
                            }
                            catch (Exception ex)
                            {
                                UpdateInfo(String.Format("Fail download action {0}: {1}", actionId, ex.Message), MyUtil.UTIL.InfoType.error);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool DownloadAction(byte actionId, bool fullDownload)
        {
            data.ActionInfo ai = actionTable.action[actionId];

            // A9 9A 03 61 00 66 ED
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.GET_ADHEADER, actionId, 0, 0xED };
            //if (V2_SendCommand(command, true, 1000))
            if (SendRobotCommand(command, CONST.CB.GET_ADHEADER))
            {
                if (receiveBuffer.Count != CONST.AI.HEADER.SIZE)
                {
                    return false;
                }
                if (ai.ReadHeaderFromArray(receiveBuffer.ToArray()))
                {
                    if (fullDownload)
                    {
                        if (ai.poseCnt > 0)
                        {
                            ai.SetPoseSize(ai.poseCnt);
                            for (UInt16 pId = 0; pId < ai.poseCnt; pId++)
                            {
                                DownloadActionPose(actionId, pId);

                            }
                        }
                        ai.poseLoaded = true;
                        ai.CheckPoses();
                    }
                    else
                    {
                        ai.poseLoaded = false;
                    }
                }
            }
            return true;
        }

        public bool DownloadActionPose(byte actionId, UInt16 poseId)
        {
            data.PoseInfo pi = actionTable.action[actionId].pose[poseId];
            bool success = false;

            byte poseHigh = (byte)((poseId >> 8) & 0xFF);
            byte poseLow = (byte)(poseId & 0xFF);
            byte[] command = { 0xA9, 0x9A, 0x05, CONST.CMD.GET_ADPOSE, actionId, poseHigh, poseLow, 0, 0xED };
            //if (V2_SendCommand(command, true, 1000))
            if (SendRobotCommand(command, CONST.CB.GET_ADPOSE))
            {
                success = pi.ReadFromArray(receiveBuffer.ToArray(), actionId, poseId);
            }
            return success;
        }


        public bool UploadAction(int actionId)
        {

            if ((actionId < 0) || (actionId >= CONST.AI.MAX_ACTION))
            {
                UpdateInfo(String.Format("Invalid action ID: {0}", actionId), MyUtil.UTIL.InfoType.error);
                return false;
            }
            data.ActionInfo ai = actionTable.action[actionId];
            byte[] header = ai.GetData();
            // TODO: Error handling
            // For safety, wait 1s for writing header
            if (!V2_TryCommand(header, 1000))
            {
                UpdateInfo(String.Format("上传动作档 {0} 的头文件 出错, 动作可能已损坏, 请再次上传.", ai.actionCode), MyUtil.UTIL.InfoType.error);
                return false;
            }
            UInt16 poseCnt = ai.poseCnt;
            for (UInt16 pId = 0; pId < poseCnt; pId++)
            {
                byte[] poseData = ai.GetPoseData(pId);
                // For safety, wait 1s for writing data
                if (!V2_TryCommand(poseData, 1000))
                {
                    UpdateInfo(String.Format("上传动作档 {0} 出错, 动作可能已损坏, 请再次上传.", ai.actionCode), MyUtil.UTIL.InfoType.error);
                    return false;
                }
            }
            UpdateInfo(String.Format("Action {0} has been uploaded", ai.actionCode));

            return true;
        }

        public byte DeleteActionFile(byte actionId)
        {
            byte result = 0xFF;
            // A9 9A 03 75 00 43 ED
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.DEL_ACTION, actionId, 0, 0xED };
            //if (V2_SendCommand(command, true, 1000))
            if (SendRobotCommand(command, CONST.CB.DEL_ACTION))
            {
                result = receiveBuffer[4];
            }
            return result;
        }


        /*
        // A9 9A 03 F1 02 F6 ED
        public bool ReadFromSPIFFS(byte actionId)
        {
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.READSPIFFS, 0, 0xED };
            if (!V2_SendCommand(command, true))
            {
                UpdateInfo(String.Format("Fail reading action {0} from SPIFFS", actionId), Util.InfoType.error);
                return false;
            }
            UpdateInfo(String.Format("Action {0} has been refresh from SPIFFS", actionId));
            return true;
        }

        public bool WriteToSPIFFS()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.WRITESPIFFS, 0, 0xED };
            if (!V2_SendCommand(command, true, 2000))
            {
                UpdateInfo("Fail writing to SPIFFS", Util.InfoType.error);
                return false;
            }
            UpdateInfo("Action data saved to SPIFFS");
            return true;
        }
        */

        public bool SetPose(int actionId, int poseId)
        {
            UpdateInfo("Please wait......");
            data.PoseInfo pi = actionTable.action[actionId].pose[poseId];
            V2_SetPose(pi);
            /*
             * // TODO: how to set delay....
             * // There has no need to wait
            */
            // int waitTimeMs = 1000 * pi.servoTime / 40 + 10;  // Add extra 10ms for safety
            int waitTimeMs = pi.servoTime + 10;
            Thread.Sleep(waitTimeMs);
            UpdateInfo("");
            RefreshAngle();
            return true;
        }

        public bool UpdatePose(int actionId, int poseId, uc.UcServo[] servo, bool allServo, byte headLed,
                               byte mp3Folder, byte mp3File, byte mp3Vol,
                               UInt16 servoTime, UInt16 waitTime)
        {
            data.PoseInfo pi = actionTable.action[actionId].pose[poseId];
            if (pi == null) return false;
            byte[] servoAngle = new byte[CONST.MAX_SERVO + 1];
            byte[] servoLed = new byte[CONST.MAX_SERVO + 1];
            for (int i = 1; i <= CONST.MAX_SERVO; i++)
            {
                if (servo[i].exists && (allServo || servo[i].isSelected))
                {
                    servoAngle[i] = servo[i].angle;
                    servoLed[i] = servo[i].led;
                }
                else
                {
                    servoAngle[i] = 0xff;
                    servoLed[i] = 0x00;
                }
            }
            return pi.Update(actionId, poseId, servoAngle, servoLed, headLed, mp3Folder, mp3File, mp3Vol, servoTime, waitTime);
        }

        // Format: Command + hex data
        //     Eg:  M 01 00 
        public void SendTextCommand(string inputStr)
        {
            int MAX_DISPLAY = 50;

            UpdateInfo("");
            inputStr = inputStr.Trim();
            if ((inputStr == null) || (inputStr == ""))
            {
                UpdateInfo("Command is empty", MyUtil.UTIL.InfoType.alert);
                return;
            }

            byte[] command;

            if (inputStr.StartsWith("\""))
            {
                // send string comamnd
                string sCommand;
                sCommand = inputStr.Substring(1);
                command = Encoding.ASCII.GetBytes(sCommand);
            }
            else
            {
                // String[] input = inputStr.Split(' ');
                Regex rx = new Regex("\\s+");
                string input = rx.Replace(inputStr, " ");
                string[] sData = input.Split(' ');
                List<Byte> lCommand = new List<byte>();

                try
                {
                    for (int i = 0; i < sData.Length; i++)
                    {
                        lCommand.Add(Util.GetInputByte(sData[i]));
                    }
                    command = lCommand.ToArray();
                }
                catch (Exception ex)
                {
                    UpdateInfo(ex.Message, MyUtil.UTIL.InfoType.error);
                    return;
                }

            }

            // Don't know how many bytes will received, wait 1s for the result
            // SendCommand(command, command.Length, 100, 1000);
            SendRobotCommand(command, 10, 1000);
            int size = (receiveBuffer.Count > MAX_DISPLAY ? MAX_DISPLAY : receiveBuffer.Count);
            StringBuilder hex = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                hex.AppendFormat("{0:x2} ", receiveBuffer[i]);
            }
            string output = hex.ToString();
            if (receiveBuffer.Count > MAX_DISPLAY) output += " ...";
            UpdateInfo(output);
        }

        public byte GetServoType()
        {
            // A9 9A 02 10 12 ED
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.SERVOTYPE, 0, 0xED };
            // if (!V2_SendCommand(command, true, 1000)) return 0;
            if (!SendRobotCommand(command, CONST.CB.SERVOTYPE)) return 0;
            if (receiveBuffer[2] != 3) return 0;
            return (receiveBuffer[4]);
        }

        public bool GetEventHandlerMode()
        {
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.GET_EH_MODE, 0, 0xED };
            V2_SendCommandWithSingleReturn(command);
            if (receiveBuffer.Count != 7) return false;
            return (receiveBuffer[4] == 1);
        }

        public bool SetEventHandlerMode(bool mode)
        {
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.SET_EH_MODE, (byte) (mode ? 1 : 0), 0, 0xED };
            if (!V2_SendCommandWithSingleReturn(command)) return false;
            return (receiveBuffer[4] == 1);
        }

        public bool CheckBattery(out UInt16 value, out byte power)
        {
            value = 0;
            power = 0;
            // A9 9A 02 0B 0D ED
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.CHECKBATTERY, 0, 0xED };
            // if (!V2_SendCommand(command, true, 1000)) return false;
            if (!SendRobotCommand(command, CONST.CB.CHECK_BATTERY)) return false;
            if (receiveBuffer[2] != 5) return false;
            power = receiveBuffer[4];
            value = Util.getUInt16(receiveBuffer, 5);
            return true;
        }


        public void CheckMpu()
        {
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CB.CHK_MPU.code, 0, 0xED };
            mpuExists = V2_SendCommandWithSingleReturn(command);
        }

        public bool GetMpuData(out Int16 ax, out Int16 ay, out Int16 az)
        {
            ax = ay = az = 0;
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CB.GET_MPU_DATA.code, 0, 0xED };
            if (!SendRobotCommand(command, CONST.CB.GET_MPU_DATA)) return false;
            // A9 9A len code {ax-4,5} {ay-6,7} {az-8,9}
            // Convert from High/Low to Low/High
            // TODO: use low / high for communizaiton in next version
            byte[] data = new byte[2];
            data[0] = receiveBuffer[5];
            data[1] = receiveBuffer[4];
            ax = BitConverter.ToInt16(data,0);
            data[0] = receiveBuffer[7];
            data[1] = receiveBuffer[6];
            ay = BitConverter.ToInt16(data, 0);
            data[0] = receiveBuffer[9];
            data[1] = receiveBuffer[8];
            az = BitConverter.ToInt16(data, 0);
            return true;
        }



        public bool PlayAction(byte actionId)
        {
            // A9 9A 03 41 00 43 ED
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.PLAYACTION, actionId, 0, 0xED };
            return V2_TryCommand(command, 1000);
        }

        public bool StopAction()
        {
            // A9 9A 02 4F 51 ED
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.STOPACTION, 0, 0xED };
            return V2_TryCommand(command, 1000);
        }

        public bool GetNetwork(out byte mode, out string ssid, out string ip, out UInt16 port)
        {
            // A9 9A 02 0C 0E ED
            mode = 0;
            ssid = "";
            ip = "";
            port = 0;
            byte[] command = { 0xA9, 0x9A, 0x02, CONST.CMD.GETNETWORK, 0, 0xED };
            int cnt = 0;
            bool ready = false;
            while (cnt++ < DEFAULT_TRY_COUNT)
            {
                // if (V2_SendCommand(command, true, 1000))
                if (SendRobotCommand(command, CONST.CB.GET_NETWORK))
                {
                    if (receiveBuffer[2] == 0x38)
                    {
                        ready = true;
                        mode = receiveBuffer[4];
                        StringBuilder sb = new StringBuilder();
                        for (int i = 5; i < 25; i++)
                        {
                            if (receiveBuffer[i] == 0) break;
                            sb.Append(Convert.ToChar(receiveBuffer[i]));
                        }
                        ssid = sb.ToString();

                        sb = new StringBuilder();
                        for (int i = 25; i < 45; i++)
                        {
                            if (receiveBuffer[i] == 0) break;
                            sb.Append(Convert.ToChar(receiveBuffer[i]));
                        }
                        ip = sb.ToString();

                        port = (UInt16)(receiveBuffer[45] << 8 | receiveBuffer[46]);
                    }
                }
            }

            return ready;
        }


        public bool SetDebug(bool mode)
        {
            // A9 9A 03 02 ?? 05 ED
            byte[] command = { 0xA9, 0x9A, 0x03, CONST.CMD.DEBUG, (byte)(mode ? 1 : 0), 0, 0xED };
            return V2_TryCommand(command, 1000);
        }


        public bool V2_SendCommandWithSingleReturn(byte[] command, long maxMs = DEFAULT_COMMAND_TIMEOUT)
        {
            // if (!V2_SendCommand(command, true, maxMs)) return false;
            if (!SendRobotCommand(command, CONST.CB.SINGLE_RETURN_SIZE, maxMs)) return false;
            return ((receiveBuffer.Count == 7) && (receiveBuffer[4] == 0));
        }

        public bool V2_TryCommand(byte[] command, long maxMs = DEFAULT_COMMAND_TIMEOUT, int tryCnt = DEFAULT_TRY_COUNT)
        {
            int cnt = 0;
            while (cnt++ < tryCnt)
            {
                if (V2_SendCommandWithSingleReturn(command, maxMs)) return true;
            }
            return false;
        }

        public bool CheckEventHandler()
        {
            eventHandlerEnabled = GetEventHandlerMode();
            return eventHandlerEnabled;
        }
    }
}
