using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace MyAlphaRobot
{
    [Serializable]
    public class ConfigObject
    {
        public int max_servo { get; set; }
        public List<Point> servos { get; set; }
        public string imagePath { get; set; }
        public bool appResource { get; set; }
        public byte[] imageData { get; set; }

        public void setDefult()
        {
            max_servo = 16;
            imagePath = "pack://application:,,,/images/alpha1s_300x450.png";
            appResource = true;
            for (int i = 0; i < max_servo; i++)
            {
                servos.Add(new Point(0,0));
            }
        }

        public ImageBrush getImageBrush()
        {
            if (this.appResource)
            {
                return new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(imagePath)),
                    Stretch = Stretch.Uniform
                };
            }
            if ((imageData == null) || (imageData.Length == 0))
            {
                // Should not happen for this case
                if (imagePath == "") return null;
                if (!File.Exists(imagePath)) return null;
                imageData = File.ReadAllBytes(imagePath);
                // no way, still cannot read
                if ((imageData == null) || (imageData.Length == 0)) return null;
            }

            BitmapSource bitmap = (BitmapSource)new ImageSourceConverter().ConvertFrom(imageData);
            return new ImageBrush()
            {
                ImageSource = bitmap
            };
        }

        public bool ChangeImage(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            byte[] readBytes;
            try
            {
                readBytes = File.ReadAllBytes(fileName);
            } catch
            {
                return false;
            }
            imagePath = fileName;
            appResource = false;
            imageData = (byte[]) readBytes.Clone();
            return true;
        }

        public ConfigObject Clone()
        {
            return UTIL.Clone<ConfigObject>(this);
        }

        public bool ToFile(String fileName)
        {
            return UTIL.FILE.SaveDataFile(this, fileName, true);
        }

        public static ConfigObject FromFile(String fileName)
        {
            return UTIL.FILE.RestoreDataFile<ConfigObject>(fileName, true);
        }
    }
}
