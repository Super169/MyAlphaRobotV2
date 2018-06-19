using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        private uc.UcServo[] servo = new uc.UcServo[CONST.MAX_SERVO + 1];

        private Random RAND = new Random(DateTime.Now.Millisecond);
        private int activeServo = 0;

        private bool batteryUpdating = false;
        private bool systemWorking = false;



        private void InitObjects()
        {
            FindPorts((string)UTIL.ReadRegistry(UTIL.KEY.LAST_CONNECTION));
            InitUBT();
            InitServo();
            InitTimer();
            InitUI();
        }

        private void InitUI()
        {
            ucComboDisplay.InitObject(UBT.comboTable, UpdateInfo);

            string lastLayout = (string)UTIL.ReadRegistry(UTIL.KEY.LAST_LAYOUT);
            wrapMain.Width = (lastLayout == "LOW" ? 600 : 300);
            ucActionList.InitObject(UBT.actionTable, UpdateInfo);
            ucActionList.DoubleClick += ActionList_DoubleClick;
            ucActionList.PlayAction += ActionList_PlayAction;
            ucActionList.StopAction += ActionList_StopAction;

            ucActionList.InsertPose += ActionList_InsertPose;
            ucActionList.InsertPoseBefore += ActionList_InsertPoseBefore;
            ucActionList.InsertPoseAfter += ActionList_InsertPoseAfter;
            ucActionList.DeletePose += ActionList_DeletePose;

            ucActionDetail.InitObject(UBT.actionTable);
            ucActionDetail.DoubleClick += ActionDetail_DoubleClick;
            ucActionDetail.EnableChanged += ActionDetail_EnableChanged;
            SetStatus();
            rbServoLock.IsChecked = true;
        }


        private void SetStatus()
        {
            bool connected = UBT.connected;
            portsComboBox.IsEnabled = !connected;
            findPortButton.IsEnabled = !connected;
            findPortButton.Visibility = (connected ? Visibility.Hidden : Visibility.Visible);
            connectButton.Content = (connected ? "断开" : "联机");
            tbCommand.IsEnabled = connected;
            sendButton.IsEnabled = connected;
            gridAction.IsEnabled = connected;
            configRobotMenuItem.IsEnabled = !connected;
            imgConfigRobot.IsEnabled = !connected;
            // saveConfigMenuItem.IsEnabled = connected;
            // loadConfigMenuItem.IsEnabled = connected;
        }

        private void InitServo()
        {
            ConfigObject CONFIG = SYSTEM.configObject;
            gridRobot.Background = loadImage(CONFIG);

            for (byte i = 1; i <= CONFIG.max_servo; i++)
            {
                uc.UcServo us = new uc.UcServo(UBT.GetServo(i));
                us.Width = 38;
                us.Height = 38;
                us.HorizontalAlignment = HorizontalAlignment.Left;
                us.VerticalAlignment = VerticalAlignment.Top;
                // us.Margin = new Thickness(servoPos[i, 0], servoPos[i, 1], 0, 0);
                us.Margin = new Thickness(CONFIG.servos[i - 1].X, CONFIG.servos[i - 1].Y, 0, 0);
                us.Show();
                us.MouseDownEventHandler += Servo_MouseDown;
                servo[i] = us;
                gridRobot.Children.Add(us);
            }

        }



    }
}
