using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MyUtil;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        // private byte[,,] actionTable = new byte[MAX_ACTION,MAX_POSES,MAX_POSE_SIZE];

        private void tb_PreviewInteger(object sender, TextCompositionEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewInteger(ref e);
        }

        private void tb_PreviewIP(object sender, TextCompositionEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewIP(ref e);
        }

        private void tb_PreviewHex(object sender, TextCompositionEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewHex(ref e);
        }

        private void tb_PreviewKeyDown_nospace(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewKeyDown_nospace(ref e);
        }

        private void UpdateActiveServoInfo()
        {
            bool activeReady = (activeServo > 0);
            tbActiveServoId.Text = (activeReady ? activeServo.ToString() : "");
            gridServo.IsEnabled = activeReady;
            // ucMainServo.tbActiveAdjServoId.Text = (activeReady ? activeServo.ToString() : "");
            // ucMainServo.gridServoFix.IsEnabled = activeReady;

            if (activeReady)
            {
                cbxActiveLocked.IsChecked = servo[activeServo].locked;
                cbxActiveSelected.IsChecked = servo[activeServo].isSelected;
                sliderActiveAngle.Value = servo[activeServo].angle;
                tbAngle.Text = servo[activeServo].angle.ToString();
                rbLedNoChange.IsChecked = (servo[activeServo].led == CONST.LED.NO_CHANGE);
                rbLedTurnOn.IsChecked = (servo[activeServo].led == CONST.LED.TURN_ON);
                rbLedTurnOff.IsChecked = (servo[activeServo].led == CONST.LED.TURN_OFF);
                // ucMainServo.txtAdjPreview.Text = servo[activeServo].angle.ToString();
                // ucMainServo.txtFixAngle.Text = servo[activeServo].angle.ToString();
            }
            else
            {
                cbxActiveLocked.IsChecked = false;
                cbxActiveSelected.IsChecked = false;
                sliderActiveAngle.Value = 0;
                tbAngle.Text = "";
                // ucMainServo.txtAdjPreview.Text = "";
                // ucMainServo.txtFixAngle.Text = "";
            }
            // ucMainServo.ReadAdjAngle();
            ucMainServo.UpdateActiveServoInfo((activeReady ? servo[activeServo].angle : (byte) 0));

        }


        private void SetSelectedServoLock(bool goLock)
        {
            List<data.ServoInfo> lsi = new List<data.ServoInfo>();
            for (byte i = 1; i <= CONST.MAX_SERVO ; i++)
            {
                if (servo[i].isSelected) lsi.Add(new data.ServoInfo(i));

            }
            UBT.LockServo(lsi, goLock);
            UpdateActiveServoInfo();
        }

        private void SetAllServoSelection(bool selected)
        {
            for (int i = 1; i <= CONST.MAX_SERVO; i++)
            {
                servo[i].SetSelection(selected);
            }
            UpdateActiveServoInfo();
        }

        private void UpdateInfoCallback(string msg, MyUtil.UTIL.InfoType iType)
        {
            UpdateInfo(msg, iType, false);
        }


        private void UpdateInfo(string msg = "", MyUtil.UTIL.InfoType iType = MyUtil.UTIL.InfoType.message, bool async = false)
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                if (async)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateInfo(msg, iType, async)));
                    return;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateInfo(msg, iType, async)));
                    return;
                }
            }
            // Update UI is allowed here
            switch (iType)
            {
                case MyUtil.UTIL.InfoType.message:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
                    break;
                case MyUtil.UTIL.InfoType.alert:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xCA, 0x51, 0x00));
                    break;
                case MyUtil.UTIL.InfoType.error:
                    statusBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
                    break;

            }
            statusInfoTextBlock.Text = msg;
        }

        private void UpdateServoCallback(byte id)
        {
            UpdateServo(id);
        }

        private void UpdateServo(byte id, bool async = false)
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                if (async)
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateServo(id)));
                    return;
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateServo(id)));
                    return;
                }
            }
            servo[id].Show();
            if (id == activeServo)
            {
                sliderActiveAngle.Value = servo[id].angle;
            }
        }


        private void CommandHandler(string command, Object parm)
        {
            TypeCode tc = (parm == null ? TypeCode.Empty : Type.GetTypeCode(parm.GetType()));
            switch (command)
            {
                case CONST.COMMAND.StartSystemWork:
                    {
                        bool updateUI = (tc == TypeCode.Boolean ? (bool)parm : false);
                        StartSystemWork(updateUI);
                    }
                    break;
                case CONST.COMMAND.EndSystemWork:
                    {
                        bool updateUI = (tc == TypeCode.Boolean ? (bool)parm : false);
                        EndSystemWork(updateUI);
                    }
                    break;
                    
            }
        }


        #region "Validation Rules"

        public bool getMp3Folder(out byte value)
        {
            if (!Util.getByte(tbMp3Folder.Text, out value, 0xFF))
            {
                UpdateInfo("音量設定不正確. 可設定 空白, 0-255.  (空白 / 255 表示根目錄)", MyUtil.UTIL.InfoType.error);
                return false;
            }
            return true;
        }

        public bool getMp3File(out byte value, bool allowEmpty)
        {

            if (!Util.getByte(tbMp3File.Text, out value,allowEmpty, 0xFF))
            {
                UpdateInfo("檔案編號不正確.  請輸入 0-255 的編號.", MyUtil.UTIL.InfoType.error);
                return false;
            }
            if (!allowEmpty && (value == 0xFF))
            {
                UpdateInfo("請設定檔案編號. 255 預設為不播放.", MyUtil.UTIL.InfoType.alert);
                return false;
            }
            return true;
        }

        public bool getMp3Vol(out byte value)
        {

            if (!Util.getByte(tbMp3Vol.Text, out value, 0xFF) || ((value > 30) && (value != 0xFF)))
            {
                UpdateInfo("音量設定不正確. 可設定 空白, 0-30, 或 255.  (空白 / 255 表示音量不變)", MyUtil.UTIL.InfoType.error);
                return false;
            }
            return true;
        }

        public bool getActionTime(out UInt16 servoTime, out UInt16 waitTime)
        {
            servoTime = 0;
            waitTime = 0;

            if ((tbExecTime.Text == "") || (tbWaitTime.Text == ""))
            {
                UpdateInfo("執行時間 及 等待時間必須填上", MyUtil.UTIL.InfoType.alert);
                return false;
            }

            if (!int.TryParse(tbExecTime.Text, out int iServoTime)) iServoTime = -1;
            if (!int.TryParse(tbWaitTime.Text, out int iWaitTime)) iWaitTime = 0;

            if ((iServoTime < 0)  || (iServoTime > 65535))
            {
                UpdateInfo("執行時間 不正確, 必須為 0-65535", MyUtil.UTIL.InfoType.alert);
                return false;
            }

            if ((iWaitTime < 0) || (iWaitTime > 65535))
            {
                UpdateInfo("等待時間 不正確, 必須為 0-65535", MyUtil.UTIL.InfoType.alert);
                return false;
            }

            servoTime = (UInt16)iServoTime;
            waitTime = (UInt16) iWaitTime;

            return true;
        }

        #endregion

    }
}
