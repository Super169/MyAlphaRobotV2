using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyAlphaRobot.uc
{
    /// <summary>
    /// Interaction logic for UcMain_ControlBoard.xaml
    /// </summary>
    public partial class UcMain_ControlBoard : UcMain__parent
    {


        public UcMain_ControlBoard()
        {
            InitializeComponent();
            SetDeveloperMode();
        }

        public void SetDeveloperMode()
        {
            Visibility vis = (SYSTEM.sc.developerMode ? Visibility.Visible : Visibility.Hidden);
            Visibility gridVis = (SYSTEM.sc.developerMode ? Visibility.Visible : Visibility.Collapsed);

            btnDebugOn.Visibility = vis;
            btnDebugOff.Visibility = vis;
            gridBatteryVoltage.Visibility = gridVis;
            gridBatteryAlarm.Visibility = gridVis;
            lblMaxDetectRetry.Visibility = vis;
            tbMaxDetectRetry.Visibility = vis;
            gridTimeoutRetry.Visibility = gridVis;

            gridMpuSensitivity.Visibility = gridVis;

            lblTouchDetectPeriod.Visibility = vis;
            tbTouchDetectPeriod.Visibility = vis;
            lblTouchReleasePeriod.Visibility = vis;
            tbTouchReleasePeriod.Visibility = vis;

            lblPsxCheckMs.Visibility = vis;
            tbPsxCheckMs.Visibility = vis;

            gridPsxInteval.Visibility = gridVis;

            lblRouterTimeout.Visibility = vis;
            tbRouterTimeout.Visibility = vis;

            lblServerPort.Visibility = vis;
            tbServerPort.Visibility = vis;
            lblUdpRxPort.Visibility = vis;
            tbUdpRxPort.Visibility = vis;
            lblUdpTxPort.Visibility = vis;
            tbUdpTxPort.Visibility = vis;

            gridSonicSettings.Visibility = gridVis;
            gridMazeSettings.Visibility = gridVis;
            gridMazeServo.Visibility = gridVis;
            gridMazeTime.Visibility = gridVis;
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

        public  void RefreshBoardConfig()
        {
            if (UBT.robotInfo.version == "")
            {
                tbVersion.Text = "請盡快更新固件";
                tbVersion.Foreground = new SolidColorBrush(Colors.Red);
            } else
            {
                tbVersion.Text = "v" + UBT.robotInfo.version;
                if (RCVersion.IsOutdated())
                {
                    tbVersion.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    tbVersion.Foreground = new SolidColorBrush(Colors.DarkBlue);
                }
            }

            // Basic Settings
            tbBatteryRefVoltage.Text = UBT.config.batteryRefVoltage.ToString();
            tbBatteryMinValue.Text = UBT.config.batteryMinValue.ToString();
            tbBatteryMaxValue.Text = UBT.config.batteryMaxValue.ToString();
            tbBatteryCheckSec.Text = UBT.config.batteryCheckSec.ToString();
            tbBatteryAlarmSec.Text = UBT.config.batteryAlarmSec.ToString();

            tbMaxServo.Text = UBT.config.maxServo.ToString();
            tbMaxDetectRetry.Text = UBT.config.maxDetectRetry.ToString();
            tbCommandTimeout.Text = UBT.config.commandTimeout.ToString();
            tbMaxCommandRetry.Text = UBT.config.maxCommandRetry.ToString();

            cbxMp3Enabled.IsChecked = UBT.config.mp3Enabled;
            tbMp3Volume.Text = UBT.config.mp3Volume.ToString();
            tbMp3Startup.Text = UBT.config.mp3Startup.ToString();
            tbStartupAction.Text = UBT.config.startupAction.ToString();

            cbxEnableOLED.IsChecked = UBT.config.enableOLED;

            cbxTouchEnabled.IsChecked = UBT.config.touchEnabled;
            tbTouchDetectPeriod.Text = UBT.config.touchDetectPeriod.ToString();
            tbTouchReleasePeriod.Text = UBT.config.touchReleasePeriod.ToString();

            cbxMpuEnabled.IsChecked = UBT.config.mpuEnabled;
            tbMpuCheckFreq.Text = UBT.config.mpuCheckFreq.ToString();
            tbMpuPositionCheckFreq.Text = UBT.config.mpuPositionCheckFreq.ToString();

            cbxPsxEnabled.IsChecked = UBT.config.psxEnabled;
            tbPsxCheckMs.Text = UBT.config.psxCheckMs.ToString();
            tbPsxNoEventMs.Text = UBT.config.psxNoEventMs.ToString();
            tbPsxIgnoreRepeatMs.Text = UBT.config.psxIgnoreRepeatMs.ToString();
            cbxPsxShock.IsChecked = UBT.config.psxShock;

            cbxSonicEnabled.IsChecked = UBT.config.sonicEnabled;
            tbSonicCheckFreq.Text = UBT.config.sonicCheckFreq.ToString();
            tbSonicDelaySec.Text = UBT.config.sonicDelaySec.ToString();

            tbMazeWallDistance.Text = UBT.config.mazeWallDistance.ToString();
            tbMazeServo.Text = UBT.config.mazeServo.ToString();
            cbxMazeServoDirection.IsChecked = UBT.config.mazeServoReverseDirection;
            tbMazeServoMoveMs.Text = UBT.config.mazeServoMoveMs.ToString();
            tbMazeServoWaitMs.Text = UBT.config.mazeServoWaitMs.ToString();

            cbxEnableRouter.IsChecked = UBT.networkConfig.enableRouter;
            tbSSID.Text = UBT.networkConfig.ssid;
            tbPassword.Text = UBT.networkConfig.password;
            tbRouterTimeout.Text = UBT.networkConfig.routerTimeout.ToString();

            cbxEnableAP.IsChecked = UBT.networkConfig.enableAP;
            tbAPName.Text = UBT.networkConfig.apName;
            tbAPKey.Text = UBT.networkConfig.apKey;

            cbxEnableServer.IsChecked = UBT.networkConfig.enableServer;
            tbServerPort.Text = UBT.networkConfig.serverPort.ToString();

            cbxEnableUDP.IsChecked = UBT.networkConfig.enableUDP;
            tbUdpRxPort.Text = UBT.networkConfig.udpRxPort.ToString();
            tbUdpTxPort.Text = UBT.networkConfig.udpTxPort.ToString();

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
                     
        private void btnEventHandler_Click(object sender, RoutedEventArgs e)
        {
            WinEventHandler win = new WinEventHandler(updateInfo, commandHandler);
            win.Owner = System.Windows.Window.GetWindow(this);
            if (win.InitObject(UBT))
            {
                win.ShowDialog();
            }
            win = null;
        }


        private void btnFactoryDefaultConfig_Click(object sender, RoutedEventArgs e)
        {
            if (!MessageConfirm("把主板设定重置为出厂设定?")) return;
            if (UBT.DefaultConfig())
            {
                if (UBT.GetConfig())
                {
                    RefreshBoardConfig();
                }
                else
                {
                    UpdateInfo("Fail reading board config");
                }
                MessageBox.Show("系统设定已还原为出厂设定.\n部份设定必须在重启时才生效.\n为确保系统的稳定性, 请即重启机器人.",
                                "系统设定还原成功", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
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
            if (!ValidInteger(tbBatteryRefVoltage, 1, 10000, "参考电压")) return;
            if (!ValidInteger(tbBatteryMinValue, 1, 10000, "最低电量读数")) return;
            if (!ValidInteger(tbBatteryMaxValue, 1, 10000, "最高电量读数")) return;
             if (!ValidInteger(tbBatteryCheckSec, 1, 255, "电量检测间距")) return;
            if (!ValidInteger(tbBatteryAlarmSec, 10, 255, "电量警报间距")) return;
            if (!ValidInteger(tbMaxServo, 1, 32, "舵机数目")) return;
            if (!ValidInteger(tbMp3Volume, 0, 30, "MP3音量")) return;
            if (!ValidInteger(tbStartupAction, 0, 255, "啟動動作")) return;

            if (!ValidInteger(tbTouchDetectPeriod, 1000, 5000, "触摸连击感应时间")) return;
            if (!ValidInteger(tbTouchReleasePeriod, 1000, 5000, "触摸感应间距")) return;
            if (!ValidInteger(tbMpuCheckFreq, 10, 100, "MPU 探测灵敏度")) return;
            if (!ValidInteger(tbMpuPositionCheckFreq, 20, 100, "姿态灵敏度")) return;

            if (!ValidInteger(tbPsxCheckMs, 10, 255, "PS2 按键侦测间距")) return;
            if (!ValidInteger(tbPsxNoEventMs, 10, 255, "PS2 按键無效间距")) return;
            if (!ValidInteger(tbPsxIgnoreRepeatMs, 50, 2000, "PS2 连按间距")) return;

            if (!ValidInteger(tbRouterTimeout, 10, 255, "等待时间")) return;
            if (!ValidInteger(tbServerPort, 0, 65535, "网路设定网页 连接埠")) return;
            if (!ValidInteger(tbUdpRxPort, 0, 65535, "群控 接收埠")) return;
            if (!ValidInteger(tbUdpTxPort, 0, 65535, "群控 发出埠")) return;

            if (!ValidInteger(tbSonicCheckFreq, 1, 10, "超声波 探测灵敏度")) return;
            if (!ValidInteger(tbSonicDelaySec, 0, 255, "超声波 触发後间距")) return;

            if (!ValidInteger(tbMazeServo, 0, CONST.MAX_SERVO, "迷宮使用的舵機")) return;
            if (!ValidInteger(tbMazeWallDistance, 10, 255, "迷宮中不能前進的阻擋牆距離")) return;

            if (!ValidInteger(tbMazeServoMoveMs, 100, 5000, "舵機改動方向轉動指令時間")) return;
            if (!ValidInteger(tbMazeServoWaitMs, 100, 5000, "等待舵機改動方向時間")) return;

            try
            {
                UBT.config.batteryRefVoltage = (UInt16)int.Parse(tbBatteryRefVoltage.Text);
                UBT.config.batteryMinValue = (UInt16)int.Parse(tbBatteryMinValue.Text);
                UBT.config.batteryMaxValue = (UInt16)int.Parse(tbBatteryMaxValue.Text);
                UBT.config.batteryCheckSec = byte.Parse(tbBatteryCheckSec.Text);
                UBT.config.batteryAlarmSec = byte.Parse(tbBatteryAlarmSec.Text);

                UBT.config.maxServo = byte.Parse(tbMaxServo.Text);
                UBT.config.maxDetectRetry = byte.Parse(tbMaxDetectRetry.Text);
                UBT.config.commandTimeout = byte.Parse(tbCommandTimeout.Text);
                UBT.config.maxCommandRetry = byte.Parse(tbMaxCommandRetry.Text);

                UBT.config.mp3Enabled = (cbxMp3Enabled.IsChecked == true);
                UBT.config.mp3Volume = byte.Parse(tbMp3Volume.Text);
                UBT.config.mp3Startup = byte.Parse(tbMp3Startup.Text);
                UBT.config.startupAction = byte.Parse(tbStartupAction.Text);
   
                UBT.config.enableOLED = (cbxEnableOLED.IsChecked == true);

                UBT.config.touchEnabled = (cbxTouchEnabled.IsChecked == true);
                UBT.config.touchDetectPeriod = (UInt16)int.Parse(tbTouchDetectPeriod.Text);
                UBT.config.touchReleasePeriod = (UInt16)int.Parse(tbTouchReleasePeriod.Text);

                UBT.config.mpuEnabled = (cbxMpuEnabled.IsChecked == true);
                UBT.config.mpuCheckFreq = byte.Parse(tbMpuCheckFreq.Text);
                UBT.config.mpuPositionCheckFreq = byte.Parse(tbMpuPositionCheckFreq.Text);

                UBT.config.psxEnabled = (cbxPsxEnabled.IsChecked == true);
                UBT.config.psxCheckMs = byte.Parse(tbPsxCheckMs.Text);
                UBT.config.psxNoEventMs = byte.Parse(tbPsxNoEventMs.Text);
                UBT.config.psxIgnoreRepeatMs = (UInt16)int.Parse(tbPsxIgnoreRepeatMs.Text);
                UBT.config.psxShock = (cbxPsxShock.IsChecked == true);

                UBT.config.sonicEnabled = (cbxSonicEnabled.IsChecked == true);
                UBT.config.sonicCheckFreq = byte.Parse(tbSonicCheckFreq.Text);
                UBT.config.sonicDelaySec = byte.Parse(tbSonicDelaySec.Text);

                UBT.config.mazeServo = byte.Parse(tbMazeServo.Text);
                UBT.config.mazeWallDistance = byte.Parse(tbMazeWallDistance.Text);
                UBT.config.mazeServoReverseDirection = (cbxMazeServoDirection.IsChecked == true);
                UBT.config.mazeServoMoveMs = (UInt16)int.Parse(tbMazeServoMoveMs.Text);
                UBT.config.mazeServoWaitMs = (UInt16)int.Parse(tbMazeServoWaitMs.Text);

                UBT.networkConfig.enableRouter = (cbxEnableRouter.IsChecked == true);
                UBT.networkConfig.ssid = tbSSID.Text;
                UBT.networkConfig.password = tbPassword.Text;
                UBT.networkConfig.routerTimeout = byte.Parse(tbRouterTimeout.Text);

                UBT.networkConfig.enableAP = (cbxEnableAP.IsChecked == true);
                UBT.networkConfig.apName = tbAPName.Text;
                UBT.networkConfig.apKey = tbAPKey.Text;

                UBT.networkConfig.enableServer = (cbxEnableServer.IsChecked == true);
                UBT.networkConfig.serverPort = (UInt16)int.Parse(tbServerPort.Text);

                UBT.networkConfig.enableUDP = (cbxEnableUDP.IsChecked == true);
                UBT.networkConfig.udpRxPort = (UInt16)int.Parse(tbUdpRxPort.Text);
                UBT.networkConfig.udpTxPort = (UInt16)int.Parse(tbUdpTxPort.Text);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "输入资料错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (UBT.SetConfig() && UBT.SetNetworkConfig())
            {
                MessageBox.Show("主控板设定已更新, 但部份设定必须在重启时才生效.\n为确保系统的稳定性, 请即重启机器人.",
                                "主控板设定更新成功", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            else
            {
                MessageBox.Show("主控板更新失败", "系统错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
