using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {

        #region SerialPort Event Handler

        private List<byte> receiveBuffer = new List<byte>();

        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            System.IO.Ports.SerialPort sp = sender as System.IO.Ports.SerialPort;

            if (sp == null) return;

            int bytesToRead = sp.BytesToRead;
            byte[] tempBuffer = new byte[bytesToRead];

            sp.Read(tempBuffer, 0, bytesToRead);

            receiveBuffer.AddRange(tempBuffer);
        }

        #endregion

        #region Menu Event Handler

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hiResDisplayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            wrapMain.Width = 300;
            MyUtil.UTIL.WriteRegistry(REGKEY.LAST_LAYOUT, "HIGH");
        }

        private void lowResDisplayMenuItem_Click(object sender, RoutedEventArgs e)
        {
            wrapMain.Width = 600;
            MyUtil.UTIL.WriteRegistry(REGKEY.LAST_LAYOUT, "LOW");
        }


        private void miSystemConfig_Click(object sender, RoutedEventArgs e)
        {
            WinSystemConfig win = new WinSystemConfig();
            win.Owner = this;
            win.ShowDialog();
            win = null;
            ucControlBoard.SetDeveloperMode();
            if (SYSTEM.sc.disableBatteryUpdate)
            {
                ClearBattery();
            } else
            {
                UpdateBattery();
            }
            if (SYSTEM.sc.disableMpuUpdate)
            {
                ClearMpu();
            } else
            {
                UpdateMpu();
            }
        }

        private void miConfigRobot_Click(object sender, RoutedEventArgs e)
        {
            ConfigRobot();
        }


        private void ConfigRobot()
        {
            WinRobotMaintenance win = new WinRobotMaintenance();
            win.Owner = this;
            win.ShowDialog();
            win = null;

        }


        private void miFirmwareUpdate_Click(object sender, RoutedEventArgs e)
        {
            WinFirmwareUpdate win = new WinFirmwareUpdate();
            win.Owner = this;
            win.ShowDialog();
            win = null;
        }

        private void miStm8Writer_Click(object sender, RoutedEventArgs e)
        {
            WinStm8Writer win = new WinStm8Writer();
            win.Owner = this;
            win.ShowDialog();
            win = null;
        }

        #endregion


        #region Button Event Handler

        private void btnSerialConnect_Click(object sender, RoutedEventArgs e)
        {
            ToggleConnection(CONN_MODE.Serial);
        }

        private void btnNetConnect_Click(object sender, RoutedEventArgs e)
        {
            bool ready = false;
            string ip = txtIP.Text.Trim();
            if (ip != "") 
            {
                if (ip.Split(new[] { "." }, StringSplitOptions.None).Count() == 4)
                {
                    IPAddress address;
                    ready = (IPAddress.TryParse(ip, out address));
                }
            }

            if (!ready)
            {
                MessageBox.Show("请输入一个合法的网路地址(IP address)", "网路地址错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ToggleConnection(CONN_MODE.Network);
        }

        private void imgConfigRobot_MouseDown(object sender, MouseEventArgs e)
        {
            ConfigRobot();
        }

        private void findPortButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            // FindPorts((string)portsComboBox.SelectedValue);
            robot.SetSerialPorts(portsComboBox, (string)portsComboBox.SelectedValue);
            SetStatus();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            UBT.SendTextCommand(tbCommand.Text);
        }

        private void btnDetect_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UBT.RefreshAngle();
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            SetAllServoSelection(true);
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            SetAllServoSelection(false);
        }

        private void btnUnlockSelected_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            SetSelectedServoLock(false);
        }

        private void btnLockSelected_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            SetSelectedServoLock(true);
        }

        private void btnLockAll_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UBT.LockServo(0, true);
        }

        private void btnAllLedNoChange_Click(object sender, RoutedEventArgs e)
        {
            SetAllLed(CONST.LED.NO_CHANGE);
        }

        private void btnAllLedOn_Click(object sender, RoutedEventArgs e)
        {
            SetAllLed(CONST.LED.TURN_ON);
        }

        private void btnAllLedOff_Click(object sender, RoutedEventArgs e)
        {
            SetAllLed(CONST.LED.TURN_OFF);
        }

        private void SetAllLed(byte mode)
        {
            UpdateInfo();
            for (int i = 1; i <= CONST.MAX_SERVO; i++)
            {
                servo[i].SetLED(mode);
                servo[i].Show();
            }
            if (mode == CONST.LED.TURN_ON)
            {
                UBT.V2_SetLED(0, 0);
            }
            else if (mode == CONST.LED.TURN_OFF)
            {
                UBT.V2_SetLED(0, 1);
            }
            if (activeServo > 0) UpdateActiveServoInfo();
        }


        private void btnMp3Test_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            // byte folderSeq, fileSeq, playVol;

            if (!getMp3Folder(out byte folderSeq)) return;
            if (!getMp3File(out byte fileSeq, false)) return;
            if (!getMp3Vol(out byte playVol)) return;

            // For MP3 command using 9600bps software serial, it'd be better to add 100ms delay between each command
            UBT.V2_MP3Stop();
            Thread.Sleep(100);
            if (playVol != 255)
            {
                UBT.V2_MP3SetVol(playVol);
                Thread.Sleep(100);
            }
            UBT.V2_MP3PlayFile(folderSeq, fileSeq);
            UpdateInfo(String.Format("播放 {0}档案 {1:0000}", (folderSeq == 0xFF ? "" : string.Format("{0:0000} 目录下的", folderSeq)), fileSeq));
        }

        private void btnMp3Stop_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UBT.V2_MP3Stop();
            UpdateInfo("音乐播放停止");
        }

        private void btnReadPose_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            data.PoseInfo pi = ucActionDetail.GetSelectedPose();
            if (pi == null)
            {
                UpdateInfo("没有选定姿势", MyUtil.UTIL.InfoType.alert);
            }
            else
            {
                UBT.SetPose(pi.actionId, pi.poseId);

                for (int i = 1; i <= CONST.MAX_SERVO; i++)
                {
                    servo[i].SetLED(pi.servoLed[i]);
                    servo[i].Show();
                }

                if (activeServo > 0)
                {
                    rbLedNoChange.IsChecked = (pi.servoLed[activeServo] == CONST.LED.NO_CHANGE);
                    rbLedTurnOn.IsChecked = (pi.servoLed[activeServo] == CONST.LED.TURN_ON);
                    rbLedTurnOff.IsChecked = (pi.servoLed[activeServo] == CONST.LED.TURN_OFF);
                }
                rbHeadLedNoChange.IsChecked = (pi.headLed == CONST.LED.NO_CHANGE);
                rbHeadLedTurnOn.IsChecked = (pi.headLed == CONST.LED.TURN_ON);
                rbHeadLedTurnOff.IsChecked = (pi.headLed == CONST.LED.TURN_OFF);

                if (pi.mp3Vol == CONST.AI.STOP_MUSIC_VOL)
                {
                    cbxStopMp3.IsChecked = true;
                    tbMp3Folder.Text = "";
                    tbMp3File.Text = "";
                    tbMp3Vol.Text = "";
                }
                else
                {
                    cbxStopMp3.IsChecked = false;
                    tbMp3Folder.Text = (pi.mp3Folder == 0xff ? "" : pi.mp3Folder.ToString());
                    tbMp3File.Text = (pi.mp3File == 0xff ? "" : pi.mp3File.ToString());
                    tbMp3Vol.Text = (pi.mp3Vol == 0xff ? "" : pi.mp3Vol.ToString());
                }

                tbExecTime.Text = pi.servoTime.ToString();
                tbWaitTime.Text = pi.waitTime.ToString();


            }
        }

        private void btnWritePose_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UpdatePoseInfo(false);
        }

        private void btnWritePoseAll_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UpdatePoseInfo(true);
        }

        private void btnLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            LoadActionFile();
        }

        private void btnSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            SaveActionFile();
        }

        private void btnDownloadConfig_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            DownloadActionTable();
        }
        /*
        private void btnUploadConfig_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UploadActionTable();
        }
        */
        private void btnConvertAction_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            ConvertAction();
        }

        private void btnDownloadAction_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            DownloadAction();
        }

        private void btnUploadAction_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            UploadAction();
        }

        private void btnClearAction_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            ClearAction();
        }

        private void btnDeleteAction_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            DeleteAction();
        }

        private void btnDebugOn_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            SetDebug(true);
        }

        private void btnDebugOff_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            SetDebug(false);
        }

        private void SetDebug(bool mode)
        {
            UpdateInfo();
            if (UBT.SetDebug(mode))
            {
                UpdateInfo(String.Format("除虫模式已{0}", (mode ? "启动" : "停止")));
            }
            else
            {
                UpdateInfo("设定除虫模式失败");
            }
        }

        private void UpdatePoseInfo(bool allServo)
        {
            UpdateInfo();

            data.PoseInfo pi = ucActionDetail.GetSelectedPose();
            if (pi == null)
            {
                UpdateInfo("没有选定姿势", MyUtil.UTIL.InfoType.alert);
                return;
            }

            bool lastPose = ucActionDetail.LastSelected;
            pi.enabled = true;
            byte headLed;
            if (rbHeadLedTurnOn.IsChecked == true)
            {
                headLed = CONST.LED.TURN_ON;
            }
            else if (rbHeadLedTurnOff.IsChecked == true)
            {
                headLed = CONST.LED.TURN_OFF;
            }
            else
            {
                headLed = CONST.LED.NO_CHANGE;
            }

            byte mp3Folder, mp3File, mp3Vol;
            if (cbxStopMp3.IsChecked == true)
            {
                mp3Folder = 0xFF;
                mp3File = 0xFF;
                mp3Vol = CONST.AI.STOP_MUSIC_VOL;  // Special code for mp3Vol = 0xFE -> Stop play

            }
            else
            {
                if (!getMp3Folder(out mp3Folder)) return;
                if (!getMp3File(out mp3File, true)) return;
                if (!getMp3Vol(out mp3Vol)) return;

            }

            if (!getActionTime(out UInt16 servoTime, out UInt16 waitTime)) return;

            if (UBT.UpdatePose(pi.actionId, pi.poseId, servo, allServo, headLed, mp3Folder, mp3File, mp3Vol, servoTime, waitTime))
            {
                UBT.actionTable.action[pi.actionId].CheckPoses();
                ucActionList.Refresh();
                ucActionDetail.Refresh();
                if (lastPose)
                {
                    ActoinList_InsertPose(InsertMode.END);
                }
            }

        }

        #endregion


        #region Windows Event Handler

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // if (UBT.connected) UBT.Disconnect();
            if (robot.isConnected) robot.Close();
        }

        #endregion

        #region Window Control Event Handler

        private void sliderActiveAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider sAngle = (Slider)sender;
            lblAngle.Content = Math.Round(sAngle.Value);
        }

        private void sliderActiveAngle_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (activeServo > 0)
            {
                sliderMode = 2;
                sliderTimer.Start();
            }
        }

        private void sliderActiveAngle_LostMouseCapture(object sender, MouseEventArgs e)
        {
            sliderMode = 1;
        }

        private void rbLed_Clicked(object sender, RoutedEventArgs e)
        {
            if (activeServo > 0)
            {
                byte mode = CONST.LED.NO_CHANGE;
                if (rbLedTurnOff.IsChecked == true)
                {
                    mode = CONST.LED.TURN_OFF;
                    UBT.V2_SetLED((byte)activeServo, 1);
                }
                else if (rbLedTurnOn.IsChecked == true)
                {
                    mode = CONST.LED.TURN_ON;
                    UBT.V2_SetLED((byte)activeServo, 0);
                }
                servo[activeServo].SetLED(mode);
                servo[activeServo].Show();
            }
        }

        private void rbHeadLed_Clicked(object sender, RoutedEventArgs e)
        {
            if (rbHeadLedTurnOff.IsChecked == true)
            {
                UBT.V2_SetHeadLED(0);
            }
            else if (rbHeadLedTurnOn.IsChecked == true)
            {
                UBT.V2_SetHeadLED(1);
            }
        }


        #endregion

        #region User Control Callback Event Handler

        private void RefreshAction(int actionId)
        {
            UBT.actionTable.action[actionId].CheckPoses();
            ucActionList.Refresh();
            ucActionDetail.Refresh();
        }

        private void ActionList_DoubleClick(object sender, EventArgs e)
        {
        }

        private void ActionList_PlayAction(object sender, EventArgs e)
        {
            int actionId;
            if ((actionId = GetSelectedActionId()) < 0) return; ;

            UpdateInfo();
            if (MessageConfirm(String.Format("在机械人播放动作 {0}?\n\n注意:\n播放动作时, 舵机角度不会自动更新\n请於动作播放後, 自行更新一次.", actionId)))
            {
                if (UBT.PlayAction((byte)actionId))
                {
                    UpdateInfo(String.Format("动作 {0} 开始播放", actionId));
                }
                else
                {
                    UpdateInfo("播放动作出错");
                }
            }
        }

        private void ActionList_StopAction(object sender, EventArgs e)
        {

            UpdateInfo();
            if (UBT.StopAction())
            {
                UpdateInfo("动作播放停止了");
            }
            else
            {
                UpdateInfo("停止播放动作出错");
            }
        }

        private void ActionDetail_DoubleClick(object sender, EventArgs e)
        {
            btnReadPose_Click(sender, null);
        }

        private void ActionDetail_EnableChanged(object sender, EventArgs e)
        {
            int actionId = ucActionList.SelectedIndex;
            UBT.actionTable.action[actionId].CheckPoses();
            ucActionList.Refresh();
        }

        private void ActionList_InsertPose(object sender, EventArgs e)
        {
            ActoinList_InsertPose(InsertMode.END);
        }

        private void ActionList_InsertPoseBefore(object sender, EventArgs e)
        {
            ActoinList_InsertPose(InsertMode.BEFORE);
        }

        private void ActionList_InsertPoseAfter(object sender, EventArgs e)
        {
            ActoinList_InsertPose(InsertMode.AFTER);

        }

        enum InsertMode
        {
            END, BEFORE, AFTER
        }

        private void ActoinList_InsertPose(InsertMode mode)
        {
            int actionId;
            if ((actionId = GetSelectedActionId()) < 0) return; ;
            data.ActionInfo ai = UBT.actionTable.action[actionId];
            if (ai.actionFileExists && !ai.poseLoaded)
            {
                MessageBoxResult mbr = MessageBox.Show("现在编辑会以新动作为基础, 先下载原来的动作吗? ", "档案尚未下载", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (mbr == MessageBoxResult.Cancel) return;
                if (mbr == MessageBoxResult.Yes)
                {
                    DownloadAction();
                    if (!ai.poseLoaded) return;
                }

            }

            int poseId;
            if (mode == InsertMode.END)
            {
                poseId = ai.pose.Length;
            }
            else
            {
                if ((poseId = IsPoseSelected()) < 0) return;
                if (mode == InsertMode.AFTER) poseId++;
            }

            int newPose = ai.InsertPose((UInt16)poseId);
            if (newPose < 0)
            {
                return;
            }
            RefreshAction(actionId);
            ucActionDetail.SetSelectedIndex(newPose);
        }

        private void ActionList_DeletePose(object sender, EventArgs e)
        {
            int actionId;
            if ((actionId = GetSelectedActionId()) < 0) return; ;

            int poseId;
            if ((poseId = IsPoseSelected()) < 0) return;

            int newPose = UBT.actionTable.action[actionId].DeletePose((UInt16)poseId);
            if (newPose < 0)
            {
                return;
            }
            RefreshAction(actionId);
            ucActionDetail.SetSelectedIndex(newPose);  // just try to set to next one

        }

        private void ActionList_CopyToEnd(object sender, EventArgs e)
        {
            int actionId;
            if ((actionId = GetSelectedActionId()) < 0) return; ;

            int poseId;
            if ((poseId = IsPoseSelected()) < 0) return;

            int newPose = UBT.actionTable.action[actionId].CopyToEnd((UInt16)poseId);
            if (newPose < 0)
            {
                return;
            }
            RefreshAction(actionId);
            ucActionDetail.SetSelectedIndex(newPose);  // just try to set to next one

        }

        private int IsPoseSelected()
        {
            int index = ucActionDetail.SelectedIndex;
            if (index < 0)
            {
                UpdateInfo("请先选择一个姿势", MyUtil.UTIL.InfoType.error);
            }
            return index;
        }

        #endregion

        #region Servo Event Handler

        private bool updating_servo_info = false;

        private void Servo_MouseDown(object sender, EventArgs e)
        {
            UpdateInfo();
            MouseButtonEventArgs me = (MouseButtonEventArgs)e;
            if (me.LeftButton == MouseButtonState.Pressed)
            {
                SetServoActive(sender, e);
                if (rbServoLock.IsChecked == true)
                {
                    ServoLockChange(sender, e);
                }
                else if (rbServoLed.IsChecked == true)
                {
                    ServoLedChange(sender, e);
                }
                else
                {
                    ServoSeletion(sender, e);
                }
            }
            else if (me.RightButton == MouseButtonState.Pressed)
            {
                ServoLedChange(sender, e);
            }
        }

        private void ServoLockChange(object sender, EventArgs e)
        {
            uc.UcServo ucServo = (uc.UcServo)sender;
            byte id = ucServo.id;
            if ((id < 1) || (id > CONST.MAX_SERVO)) return;

            bool goLock = !servo[id].locked;
            List<data.ServoInfo> lsi = new List<data.ServoInfo>();
            lsi.Add(new data.ServoInfo(id));
            UBT.LockServo(lsi, goLock);
            //            servo[id].Show();
            if (activeServo == id)
            {
                UpdateActiveServoInfo();
            }
        }

        private void ServoLedChange(object sender, EventArgs e)
        {
            uc.UcServo ucServo = (uc.UcServo)sender;
            byte id = ucServo.id;
            if ((id < 1) || (id > CONST.MAX_SERVO)) return;
            byte mode = servo[id].led;
            switch (mode)
            {
                case CONST.LED.NO_CHANGE:
                    mode = CONST.LED.TURN_ON;
                    UBT.V2_SetLED(id, 0);
                    break;
                case CONST.LED.TURN_ON:
                    mode = CONST.LED.TURN_OFF;
                    UBT.V2_SetLED(id, 1);
                    break;
                default:
                    mode = CONST.LED.NO_CHANGE;
                    break;
            }
            servo[id].SetLED(mode);
            servo[id].Show();
            if (activeServo == id)
            {
                UpdateActiveServoInfo();
            }
        }

        private void SetServoActive(object sender, EventArgs e)
        {
            updating_servo_info = true;
            uc.UcServo ucServo = (uc.UcServo)sender;
            if ((activeServo > 0) && (activeServo != ucServo.id))
            {
                servo[activeServo].SetActive(false);
            }
            ucServo.SetActive(true);
            activeServo = (ucServo.isActive ? ucServo.id : 0);
            ucMainServo.SetActiveServo(activeServo);
            UpdateActiveServoInfo();
            updating_servo_info = false;
        }

        private void ServoSeletion(object sender, EventArgs e)
        {
            updating_servo_info = true;
            uc.UcServo ucServo = (uc.UcServo)sender;

            if (ucServo.isActive)
            {
                if (rbSelect.IsChecked == true)
                {
                    ucServo.SetSelection(true);
                }
                else if (rbUnSelect.IsChecked == true)
                {
                    ucServo.SetSelection(false);
                }
                else if (rbToggle.IsChecked == true)
                {
                    ucServo.SetSelection(!ucServo.isSelected);
                }
            }
            UpdateActiveServoInfo();

            updating_servo_info = false;
            UpdateInfo(String.Format("Servo_Click from {0}", ucServo.id));
        }


        private void cbxActiveSelected_Changed(object sender, RoutedEventArgs e)
        {
            if ((!updating_servo_info) && (activeServo > 0))
            {
                servo[activeServo].SetSelection((cbxActiveSelected.IsChecked == true));
            }
        }

        private void cbxActiveLocked_Changed(object sender, RoutedEventArgs e)
        {
            if ((!updating_servo_info) && (activeServo > 0))
            {
                bool goLock = (this.cbxActiveLocked.IsChecked == true);
                List<data.ServoInfo> lsi = new List<data.ServoInfo>();
                lsi.Add(new data.ServoInfo((byte)activeServo));
                UBT.LockServo(lsi, goLock);
                UpdateActiveServoInfo();
            }
        }

        private void cbxStopMp3_Changed(object sender, RoutedEventArgs e)
        {
            if (cbxStopMp3.IsChecked == true)
            {
                tbMp3Folder.Text = "";
                tbMp3Folder.IsEnabled = false;
                tbMp3File.Text = "";
                tbMp3File.IsEnabled = false;
                tbMp3Vol.Text = "";
                tbMp3Vol.IsEnabled = false;
            }
            else
            {
                tbMp3Folder.IsEnabled = true;
                tbMp3File.IsEnabled = true;
                tbMp3Vol.IsEnabled = true;
            }
        }

        #endregion

        private void ucActionList_SelectionChanged(object sender, EventArgs e)
        {
            UpdateInfo();
            int actionId = ucActionList.GetSelectedActionId();
            if (actionId == -1) return;
            ucActionDetail.Refresh(actionId);
        }

        private void btnSuspendEventHandler_Click(object sender, RoutedEventArgs e)
        {
            SetEventHandler(false);
        }

        private void btnResumeEventHandler_Click(object sender, RoutedEventArgs e)
        {
            SetEventHandler(true);
        }

        private void SetEventHandler(bool mode)
        {
            UBT.SetEventHandlerMode(mode);
            UBT.CheckEventHandler();
            UpdateEventHandlerStatus();
        }
    }
}
