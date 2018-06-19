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
        private void InitConfig()
        {
            SystemConfig sc = UTIL.FILE.RestoreConfig<SystemConfig>(CONST.CONFIG_FILE, false);
            if (Object.ReferenceEquals(sc, null))
            {
                sc = new SystemConfig()
                {
                    robotConfig = CONST.DEFAULT_ROBOT_CONFIG
                };
            }

            if (!File.Exists(UTIL.FILE.GetConfigFileFullName(CONST.CONFIG_FILE)))
            {
                UTIL.FILE.SaveConfig(sc, CONST.CONFIG_FILE, false);
            }

            bool configReady = false;
            string fullName = UTIL.FILE.GetConfigFileFullName(sc.robotConfig);
            if (File.Exists(fullName))
            {
                try
                {
                    SYSTEM.configObject = ConfigObject.FromFile(fullName);
                    configReady = true;
                    SYSTEM.robotConfig = sc.robotConfig;
                }
                catch { }
            }

            if ((!configReady) || (Object.ReferenceEquals(SYSTEM.configObject, null)))
            {
                SYSTEM.robotConfig = "";
                SYSTEM.configObject = DefaultConfig();
            }
            CONST.MAX_SERVO = SYSTEM.configObject.max_servo;
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
            co.max_servo = 20;
            co.servos = new List<Point>();
            for (int i = 0; i < co.max_servo; i++)
            {
                co.servos.Add(new Point(servoPos[i, 0], servoPos[i, 1]));
            }
            co.imagePath = "pack://application:,,,/images/alpha1s_300x450.png";
            co.appResource = true;
            return co;
        }

        public ImageBrush loadImage(ConfigObject co)
        {
            if (co.imagePath == "") return null;
            ImageBrush image = new ImageBrush
            {
                ImageSource = (co.appResource ? new BitmapImage(new Uri(co.imagePath)) : new BitmapImage(new Uri(co.imagePath, UriKind.Relative))),
                Stretch = Stretch.Uniform
            };
            return image;
        }

    }
}
