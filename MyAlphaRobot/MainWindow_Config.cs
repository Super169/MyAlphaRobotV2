using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        /*
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
                    blocklyPath = CONST.DEFAULT_CONFIG.BLOCKLY_PATH,
                    autoCheckVersion = CONST.DEFAULT_CONFIG.AUTO_CHECK_VERSION,
                    autoCheckFirmware = CONST.DEFAULT_CONFIG.AUTO_CHECK_FIRMWARE,
                    developerMode = CONST.DEFAULT_CONFIG.DEVEOPER_MODE
                };
            } else
            {
                if (SYSTEM.sc.robotConfigFile == null) SYSTEM.sc.robotConfigFile = CONST.DEFAULT_CONFIG.ROBOT_CONFIG_FILE;
                if (SYSTEM.sc.blocklyPath == null) SYSTEM.sc.blocklyPath = CONST.DEFAULT_CONFIG.BLOCKLY_PATH;
            }

            #region "Version Config"

            SYSTEM.versionChecked = false;
            if (SYSTEM.sc.autoCheckVersion)
            {

            }

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

        public ConfigObject DefaultConfig()
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
        */

        public ImageBrush loadImage(ConfigObject co)
        {
            if (co.imagePath == "") return null;
            /*
            ImageBrush image = new ImageBrush
            {
                ImageSource = (co.appResource ? new BitmapImage(new Uri(co.imagePath)) : new BitmapImage(new Uri(co.imagePath, UriKind.Relative))),
                Stretch = Stretch.Uniform
            };
            return image;
            */
            if (co.appResource)
            {
                ImageBrush image = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(co.imagePath)),
                    Stretch = Stretch.Uniform
                };
                return image;

            }
            return co.getImageBrush();
        }

    }
}
