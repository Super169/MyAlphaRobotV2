using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MyUtil;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        private RobotConnection robot = new RobotConnection();

        private void InitConnection()
        {
            robot.InitObject(UpdateInfo);
            robot.SetSerialPorts(portsComboBox);
            robot.SetNetConnection(txtIP, txtPort);
        }

        private void SetStatus()
        {
            bool connected = robot.isConnected;
            portsComboBox.IsEnabled = !connected;
            findPortButton.IsEnabled = !connected;
            findPortButton.Visibility = (connected ? Visibility.Hidden : Visibility.Visible);
            txtIP.IsEnabled = !connected;
            txtPort.IsEnabled = !connected;
            gridConnection.Background = new SolidColorBrush((robot.isConnected ? Colors.Aqua : Colors.Gray));
            if (connected)
            {
                btnSerialConnect.Content = "断开串口";
                btnNetConnect.Content = "断开网路";
                btnSerialConnect.IsEnabled = (robot.currMode == RobotConnection.connMode.Serial);
                btnNetConnect.IsEnabled = (robot.currMode == RobotConnection.connMode.Network);
                gridSerialConnection.Visibility = (robot.currMode == RobotConnection.connMode.Serial ? Visibility.Visible : Visibility.Hidden);
                gridNetConnection.Visibility = (robot.currMode == RobotConnection.connMode.Network ? Visibility.Visible : Visibility.Hidden);
            }
            else
            {
                btnSerialConnect.Content = "串口连接";
                btnNetConnect.Content = "网路连接";
                btnSerialConnect.IsEnabled = (portsComboBox.Items.Count > 0);
                btnNetConnect.IsEnabled = true;

                gridSerialConnection.Visibility = Visibility.Visible;
                gridNetConnection.Visibility = Visibility.Visible;
            }
            tbCommand.IsEnabled = connected;
            sendButton.IsEnabled = connected;
            gridAction.IsEnabled = connected;
            miConfigRobot.IsEnabled = !connected;
            miFirmwareUpdate.IsEnabled = !connected;
            imgConfigRobot.IsEnabled = !connected;
        }

        private void UpdateEventHandlerStatus()
        {
            if (!robot.isConnected)
            {
                lblEventHandlerStatus.Visibility = Visibility.Hidden;
                btnSuspendEventHandler.Visibility = Visibility.Hidden;
                btnResumeEventHandler.Visibility = Visibility.Hidden;

                return;
            }
            if (UBT.eventHandlerEnabled)
            {
                lblEventHandlerStatus.Content = "反射动作启动中";
                lblEventHandlerStatus.Foreground = Brushes.LightGreen;
                gridEventHandlerMode.Background = Brushes.LightGreen;

            }
            else
            {
                lblEventHandlerStatus.Content = "反射动作已屏蔽";
                lblEventHandlerStatus.Foreground = Brushes.Gray;
                gridEventHandlerMode.Background = Brushes.Gray;
            }
            lblEventHandlerStatus.Visibility = Visibility.Visible;
            btnSuspendEventHandler.Visibility = (UBT.eventHandlerEnabled ? Visibility.Visible : Visibility.Hidden);
            btnResumeEventHandler.Visibility = (UBT.eventHandlerEnabled ? Visibility.Hidden : Visibility.Visible);
        }

        private enum CONN_MODE
        {
            Serial, Network
        }

        private void ToggleConnection(CONN_MODE connMode)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            UpdateInfo();
            backgroundTimer.Stop();
            bool success = false;

            if (robot.isConnected)
            {
                success = robot.Disconnect();
            }
            else
            {
                switch (connMode)
                {
                    case CONN_MODE.Serial:
                        success = robot.Connect((string)portsComboBox.SelectedValue);
                        Thread.Sleep(100); // Wait 0.1s to check if anything returned
                        if (robot.Available == 0)
                        {
                            break;
                        }
                        // Data returned immediately, possible reboot
                        UpdateInfo("Wait for reboot");
                        Thread.Sleep(SYSTEM.sc.waitRebootSec * 1000);
                        robot.ClearRxBuffer();
                        break;
                    case CONN_MODE.Network:
                        int port;
                        if (int.TryParse(txtPort.Text, out port))
                        {
                            success = robot.Connect(txtIP.Text, port);
                        }
                        break;
                }
            }
            if (success)
            {
                if (robot.isConnected)
                {
                    PostConnection();
                }
                else
                {
                    PostDisconnect();
                }
            }
            if (robot.isConnected)
            {
                byte mode;
                string ssid, ip;
                UInt16 port;
                if (UBT.GetNetwork(out mode, out ssid, out ip, out port))
                {
                    if (mode == 0)
                    {
                        lblSSID.Foreground = Brushes.Red;
                        lblSSID.Content = "No Network";
                        lblIP.Content = "";
                    }
                    else
                    {
                        lblSSID.Content = ssid;
                        lblIP.Content = String.Format("{0}:{1}", ip, port);
                        switch (mode)
                        {
                            case 1:
                                lblSSID.Foreground = Brushes.LightBlue;
                                break;
                            case 2:
                                lblSSID.Foreground = Brushes.LightPink;
                                break;
                            default:
                                lblSSID.Foreground = Brushes.Red;
                                break;
                        }
                    }
                }
                else
                {
                    lblSSID.Foreground = Brushes.Red;
                    lblSSID.Content = "Unknown Network";
                    lblIP.Content = "";
                }

                string msg = null;
                string currVersion = RCVersion.ToCode();
                if (RCVersion.IsOutdated())
                {
                    msg = "你的固件并未达到上位机的要求, 部份功能可能会出错, 必须尽快更新";
                    msg += string.Format("\n\n你的固件版本:　 {0}\n上位機最低要求: {1}", currVersion, RCVersion.TargetVersion.GetCode());
                }
                else if (SYSTEM.sc.autoCheckFirmware)
                {
                    if (string.IsNullOrWhiteSpace(UBT.robotInfo.version))
                    {
                        msg = "";
                    }
                    else
                    {
                        string latestVersion = Util.LatestVersion();
                        if (currVersion != latestVersion)
                        {
                            switch (SYSTEM.sc.firmwareType)
                            {
                                case SystemConfig.FIRMWARE.beta:
                                    msg = "开发者测试版";
                                    break;
                                case SystemConfig.FIRMWARE.hailzd:
                                    msg = "海灵专用版";
                                    break;
                                default:
                                    msg = "正式发布版";
                                    break;
                            }
                            msg += "固件己发布了新版本";
                            msg += string.Format("\n\n你的固件版本: {0}\n最新固件版本: {1}", currVersion, latestVersion);
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    msg += "\n\n请用 [机械人固件烧录] 功能, 更新你的固件";
                    MessageBox.Show(msg, "固件更新", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }

                if (UBT.config.version < data.BoardConfig.MIN_VERSION)
                {
                    MessageBox.Show(String.Format("侦测不到所需的固件 {0}, 部份功能可能会失效.\n请尝试重新连线, 或更新固件档.", RCVersion.TargetVersion.GetCode()), 
                                    "读取设定档失败或设定档已过时", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }

                if (SYSTEM.configObject.max_servo != UBT.config.maxServo)
                {
                    MessageBox.Show(string.Format("上位机跟主控板的舵机数目设定不同\n上位机: {0} 舵机, 主控板: {1} 舵机\n请确定所选的机械人模型跟主控板是一致的\n或修改有关设定\n\n完成後, 请重新启动程式再连接.\n", SYSTEM.configObject.max_servo, UBT.config.maxServo),
                                    "舵机数目不配合", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                }

                if (!SYSTEM.sc.disableBatteryUpdate) UpdateBattery();
                backgroundTimer.Start();
            }
            UpdateEventHandlerStatus();
            SetStatus();
            Mouse.OverrideCursor = null;
        }

        private void PostConnection()
        {
            UBT.PostConnection();
            ucActionList.Refresh();
            ucMainServo.CheckServoType();
            ucControlBoard.RefreshBoardConfig();
        }

        private void PostDisconnect()
        {
            UBT.PostDisconnect();
            ucActionList.Refresh();
            ucActionDetail.Refresh();
            ucControlBoard.RefreshBoardConfig();
            ClearBattery();
            ClearMpu();
        }

    }
}
