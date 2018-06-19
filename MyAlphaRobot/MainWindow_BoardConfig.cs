using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        
        private void btnFactoryDefaultConfig_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageConfirm("把主板设定重置为出厂设定?")) return;
            if (UBT.DefaultConfig())
            {
                if (UBT.GetConfig())
                {
                    RefreshBoardConfig();
                } else
                {
                    UpdateInfo("Fail reading board config");
                }
                MessageBox.Show("系统设定已还原为出厂设定.\n部份设定必须在重启时才生效.\n为确保系统的稳定性, 请即重启机器人.", 
                                "系统设定还原成功", MessageBoxButton.OK, MessageBoxImage.Warning);
            } else
            {
                UpdateInfo("Fail resetting config to factory default");
            }
        }

        private void btnResetBoardConfig_Click(object sender, RoutedEventArgs e)
        {
            RefreshBoardConfig();
        }

        private void btnSaveBoardConfig_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidInteger(tbVoltageRef, 1, 10000, "参考电压")) return;
            if (!ValidInteger(tbVoltageLow, 1, 10000, "最低电压")) return;
            if (!ValidInteger(tbVoltageHigh, 1, 10000, "最高电压")) return;
            int vLow;
            int.TryParse(tbVoltageLow.Text, out vLow);
            if (!ValidInteger(tbVoltageHigh, vLow, 10000, "最高电压")) return;
            if (!ValidInteger(tbVoltageAlarm, 1, 10000, "低电门槛")) return;
            if (!ValidInteger(tbVoltageAlarmInterval, 10, 999, "低电提示间距")) return;
            if (!ValidInteger(tbMaxServo, 1, 32, "舵机数目")) return;
            if (!ValidInteger(tbMp3Volume, 0, 30, "MP3音量")) return;
            if (!ValidInteger(tbTouchDetectPeriod, 1000, 5000, "触摸连击感应时间")) return;
            if (!ValidInteger(tbTouchReleasePeriod, 1000, 5000, "触摸感应间距")) return;
            if (!ValidInteger(tbMpuCheckFreq, 10, 255, "探测灵敏度")) return;
            if (!ValidInteger(tbPositionCheckFreq, 20, 255, "姿态灵敏度")) return;

            try
            {
                UBT.config.voltageRef = (UInt16) int.Parse(tbVoltageRef.Text);
                UBT.config.voltageLow = (UInt16)int.Parse(tbVoltageLow.Text);
                UBT.config.voltageHigh = (UInt16)int.Parse(tbVoltageHigh.Text);
                UBT.config.voltageAlarm = (UInt16)int.Parse(tbVoltageAlarm.Text);
                UBT.config.voltageAlarmMp3 = byte.Parse(tbVoltageAlarmMp3.Text);
                UBT.config.voltageAlarmInterval = byte.Parse(tbVoltageAlarmInterval.Text);
                UBT.config.maxServo = byte.Parse(tbMaxServo.Text);
                UBT.config.maxDetectRetry = byte.Parse(tbMaxDetectRetry.Text);
                UBT.config.commandTimeout = byte.Parse(tbCommandTimeout.Text);
                UBT.config.maxCommandRetry = byte.Parse(tbMaxCommandRetry.Text);
                UBT.config.enableMp3 = (cbxMp3Enabled.IsChecked == true);
                UBT.config.mp3Volume = byte.Parse(tbMp3Volume.Text);
                UBT.config.mp3Startup = byte.Parse(tbMp3Startup.Text);
                UBT.config.autoStand = (cbxAutoStand.IsChecked == true);
                UBT.config.autoStandFaceUp = byte.Parse(tbAutoFaceUp.Text);
                UBT.config.autoStandFaceDown = byte.Parse(tbAutoFaceDown.Text);
                UBT.config.connectRouter = (cbxConnectRouter.IsChecked == true);
                UBT.config.enableOLED = (cbxEnableOLED.IsChecked == true);
                UBT.config.enableTouch = (cbxTouchEnabled.IsChecked == true);
                UBT.config.touchDetectPeriod = (UInt16)int.Parse(tbTouchDetectPeriod.Text);
                UBT.config.touchReleasePeriod = (UInt16)int.Parse(tbTouchReleasePeriod.Text);
                UBT.config.setTouchAction(0, byte.Parse(tbTouchAction0.Text));
                UBT.config.setTouchAction(1, byte.Parse(tbTouchAction1.Text));
                UBT.config.setTouchAction(2, byte.Parse(tbTouchAction2.Text));
                UBT.config.setTouchAction(3, byte.Parse(tbTouchAction3.Text));
                UBT.config.mpuCheckFreq = byte.Parse(tbMpuCheckFreq.Text);
                UBT.config.positionCheckFreq = byte.Parse(tbPositionCheckFreq.Text);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "输入资料错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (UBT.SetConfig())
            {
                MessageBox.Show("主控板设定已更新, 但部份设定必须在重启时才生效.\n为确保系统的稳定性, 请即重启机器人.", 
                                "主控板设定更新成功", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                MessageBox.Show("主控板更新失败", "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshBoardConfig()
        {
            // Basic Settings
            tbVoltageRef.Text = UBT.config.voltageRef.ToString();
            tbVoltageLow.Text = UBT.config.voltageLow.ToString();
            tbVoltageHigh.Text = UBT.config.voltageHigh.ToString();
            tbVoltageAlarm.Text = UBT.config.voltageAlarm.ToString();
            tbVoltageAlarmMp3.Text = UBT.config.voltageAlarmMp3.ToString();
            tbVoltageAlarmInterval.Text = UBT.config.voltageAlarmInterval.ToString();
            tbMaxServo.Text = UBT.config.maxServo.ToString();
            tbMaxDetectRetry.Text = UBT.config.maxDetectRetry.ToString();
            tbCommandTimeout.Text = UBT.config.commandTimeout.ToString();
            tbMaxCommandRetry.Text = UBT.config.maxCommandRetry.ToString();
            cbxMp3Enabled.IsChecked = UBT.config.enableMp3;
            tbMp3Volume.Text = UBT.config.mp3Volume.ToString();
            tbMp3Startup.Text = UBT.config.mp3Startup.ToString();
            cbxConnectRouter.IsChecked = UBT.config.connectRouter;
            cbxEnableOLED.IsChecked = UBT.config.enableOLED;

            // Action
            cbxAutoStand.IsChecked = UBT.config.autoStand;
            tbAutoFaceUp.Text = UBT.config.autoStandFaceUp.ToString();
            tbAutoFaceDown.Text = UBT.config.autoStandFaceDown.ToString();
            cbxTouchEnabled.IsChecked = UBT.config.enableTouch;
            tbTouchDetectPeriod.Text = UBT.config.touchDetectPeriod.ToString();
            tbTouchReleasePeriod.Text = UBT.config.touchReleasePeriod.ToString();

            tbTouchAction0.Text = UBT.config.touchAction(0).ToString();
            tbTouchAction1.Text = UBT.config.touchAction(1).ToString();
            tbTouchAction2.Text = UBT.config.touchAction(2).ToString();
            tbTouchAction3.Text = UBT.config.touchAction(3).ToString();

            tbMpuCheckFreq.Text = UBT.config.mpuCheckFreq.ToString();
            tbPositionCheckFreq.Text = UBT.config.positionCheckFreq.ToString();
        }

        private bool ValidInteger(TextBox tb, int min, int max, string fieldName)
        {
            int value;
            bool valid = int.TryParse(tb.Text, out value);
            if ((value >= min) && (value <= max)) return true;
            string msg = String.Format("{0} 的數值 '{3}' 不正確\n\n請輸入 {1} 至 {2} 之間的數值.", fieldName, min, max, tb.Text);
            MessageBox.Show(msg, "輸入錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

    }
}
