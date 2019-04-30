using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MyAlphaRobot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
        public static DateTime buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(
                                               TimeSpan.TicksPerDay * version.Build + // days since 1 January 2000
                                               TimeSpan.TicksPerSecond * 2 * version.Revision)); // seconds since midnight, (multiply by 2 to get original) 
        public string winTitle = string.Format("MyAlphaRobot v{0}  [{1:yyyy-MM-dd HH:mm}]  (No Copyright © 2018 Super169)", version, buildDateTime);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            InitConfig();
            CefSharp.Cef.Initialize(new CefSharp.Wpf.CefSettings());
            MyUtil.UTIL.KEY.AppName = "MyAlphaRobot";
            MainWindow = new MainWindow();
            SYSTEM.main = (MainWindow) MainWindow;
            MainWindow.Title = winTitle;
            MainWindow.Closing += MainWindow_Closing;
            ShowMainWindow();
        }

        public void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            CefSharp.Cef.Shutdown();
        }

        public void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            ((MainWindow)MainWindow).OnWindowClosing(sender, e);
        }

        private void InitConfig()
        {
            SYSTEM.configPath = MyUtil.FILE.AppFilePath(CONST.CONFIG_FOLDER);
            SYSTEM.systemConfigFile = Path.Combine(SYSTEM.configPath, CONST.SYSTEM_CONFIG);

            SYSTEM.sc = MyUtil.FILE.RestoreDataFile<SystemConfig>(SYSTEM.systemConfigFile, CONST.SYSTEM_CONFIG_ZIP);
            if (Object.ReferenceEquals(SYSTEM.sc, null))
            {
                SYSTEM.sc = new SystemConfig()
                {
                    robotConfigFile = CONST.DEFAULT_CONFIG.ROBOT_CONFIG_FILE,
                    blocklyPath = MyUtil.FILE.AppFilePath(CONST.DEFAULT_CONFIG.BLOCKLY_PATH),
                    autoCheckVersion = CONST.DEFAULT_CONFIG.AUTO_CHECK_VERSION,
                    autoCheckFirmware = CONST.DEFAULT_CONFIG.AUTO_CHECK_FIRMWARE,
                    waitRebootSec = CONST.DEFAULT_CONFIG.WAIT_REBOOT_SEC,
                    disableBatteryUpdate = CONST.DEFAULT_CONFIG.DISABLE_BATTERY_UPDATE,
                    disableMpuUpdate = CONST.DEFAULT_CONFIG.DISABLE_MPU_UPDATE,
                    developerMode = CONST.DEFAULT_CONFIG.DEVEOPER_MODE
                };
            }
            else
            {
                if (SYSTEM.sc.robotConfigFile == null) SYSTEM.sc.robotConfigFile = CONST.DEFAULT_CONFIG.ROBOT_CONFIG_FILE;
                if (String.IsNullOrWhiteSpace(SYSTEM.sc.blocklyPath)) SYSTEM.sc.blocklyPath = MyUtil.FILE.AppFilePath(CONST.DEFAULT_CONFIG.BLOCKLY_PATH);
            }

            // fill default for invalid value, to handle for reading from old config files
            if ((SYSTEM.sc.waitRebootSec == 0) || (SYSTEM.sc.waitRebootSec > 99))
            {
                SYSTEM.sc.waitRebootSec = CONST.DEFAULT_CONFIG.WAIT_REBOOT_SEC;
            }

            // Check blockly path

            if (!Util.IsBlocklyPath(SYSTEM.sc.blocklyPath))
            {
                if (!Util.IsBlocklyPath(MyUtil.FILE.AppFilePath(CONST.DEFAULT_CONFIG.BLOCKLY_PATH)))
                {
                    SYSTEM.sc.blocklyPath = MyUtil.FILE.AppFilePath(CONST.DEFAULT_CONFIG.BLOCKLY_PATH);
                }
                    
            }

            #region "Version Config"

                if (SYSTEM.sc.autoCheckFirmware)
            {
                Util.CheckFirmware();
            }
            else
            {
                SYSTEM.firmwareChecked = false;
            }

            #endregion


            #region "Get Robot Config"

            if (!File.Exists(SYSTEM.systemConfigFile))
            {
                MyUtil.FILE.SaveDataFile(SYSTEM.sc, SYSTEM.systemConfigFile, CONST.SYSTEM_CONFIG_ZIP);
            }

            bool robotConfigReady = false;
            string robotConfigFile = Path.Combine(SYSTEM.configPath, SYSTEM.sc.robotConfigFile);
            if (File.Exists(robotConfigFile))
            {
                try
                {
                    SYSTEM.configObject = ConfigObject.FromFile(robotConfigFile);
                    robotConfigReady = true;
                }
                catch { }
            }

            if ((!robotConfigReady) || (Object.ReferenceEquals(SYSTEM.configObject, null)))
            {
                SYSTEM.configObject = DefaultConfig();
            }
            CONST.MAX_SERVO = SYSTEM.configObject.max_servo;

            #endregion

            if (!File.Exists(Path.Combine(SYSTEM.sc.blocklyPath, CONST.BLOCKLY.CHECK_FILE)))
            {
                SYSTEM.sc.blocklyPath = "";
            }

            Util.SaveSystemConfig();

        }

        private ConfigObject DefaultConfig()
        {
            int[,] servoPos = new int[20, 2] {
                                                {92,95},{48,95},{36,160},
                                                {174,95},{220,95},{232,160},
                                                {103,184},{97,223},{93,285},{87,343},{96,378},
                                                {163,184},{168,223},{173,285},{179,343},{170,378},
                                                {133,80},{30, 230},{236,230},{133,180}
                                                };

            ConfigObject co = new ConfigObject();
            co.max_servo = 16;
            co.servos = new List<Point>();
            for (int i = 0; i < co.max_servo; i++)
            {
                co.servos.Add(new Point(servoPos[i, 0], servoPos[i, 1]));
            }
            co.imagePath = "pack://application:,,,/images/alpha1s_300x450.png";
            co.appResource = true;
            return co;
        }
    }
}
