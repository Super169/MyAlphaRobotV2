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

        private void adjAngle_Changed(object sender, RoutedEventArgs e)
        {
            string adjAngle = txtAdjAngle.Text;
            try
            {
                int adjValue = Convert.ToInt32(adjAngle, 16);
                SetAdjAngle(adjValue);
            }
            catch (Exception)
            {
                txtAdjAngle.Background = new SolidColorBrush(Colors.LightPink);
            }
        }

        private void SetAdjAngle(int adjValue)
        {
            bool validInput = (((adjValue >= 0x0000) && (adjValue <= 0x0130)) ||
                               ((adjValue >= 0xFED0) && (adjValue <= 0xFFFF)));
            txtAdjAngle.Background = new SolidColorBrush((validInput ? Colors.White : Colors.LightPink));
            if (validInput)
            {

                sliderAdjValue.Value = (adjValue > 0x8000 ? adjValue - 65536 : adjValue);
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int n = (int)((Slider)sender).Value;

            if (n == 0)
            {
                txtAdjMsg.Text = String.Format("沒有偏移");
            }
            else if (n > 0)
            {
                txtAdjMsg.Text = String.Format("正向偏移 {0}", n);
            }
            else
            {
                txtAdjMsg.Text = String.Format("反向偏移 {0}", -n);
            }
            if (n < 0) n += 65536;
            txtAdjAngle.Text = n.ToString("X4");
        }

        private void btnReadAdjAngle_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            if (activeServo > 0)
            {
                ReadAdjAngle();
            }
        }

        private void ReadAdjAngle()
        {
            UpdateInfo();
            UInt16 adj = UBT.V2_GetAdjAngle((byte)activeServo);
            if (adj != 0x7F7F)
            {
                SetAdjAngle((int)adj);
            }
        }

        private void btnSetAdjAngle_Click(object sender, RoutedEventArgs e)
        {
            SetServoAdjAngle();
        }

        private void SetServoAdjAngle()
        {
            UpdateInfo();
            if (activeServo > 0)
            {
                byte id = (byte)activeServo;

                int n = (int)sliderAdjValue.Value;
                if (n < 0) n += 65536;
                UInt16 adjValue = (UInt16)n;

                if (!UBT.V2_SetAdjAngle(id, adjValue))
                {
                    UpdateInfo("Fail setting servo adjustment", UTIL.InfoType.error);
                    return;
                }
                if ((txtAdjPreview.Text == null) || (txtAdjPreview.Text.Trim() == "")) return;
                byte angle = (byte)Convert.ToInt32(txtAdjPreview.Text, 10);
                UBT.V2_MoveServo(id, angle, 50);
            }
        }

        private void btnAutoFixAngle_Click(object sender, RoutedEventArgs e)
        {
            if (activeServo > 0)
            {
                byte id = (byte)activeServo;
                string sFixAngle = txtFixAngle.Text.Trim();
                if (sFixAngle == "")
                {
                    UpdateInfo("Please enter current angle", UTIL.InfoType.error);
                    return;
                }
                int iFixAngle;
                if ((!int.TryParse(sFixAngle, out iFixAngle)) || (iFixAngle < 0) || (iFixAngle > 180))
                {
                    UpdateInfo("Invalid angle, should be 0 ~ 180", UTIL.InfoType.error);
                    return;
                }
                // first get current adjustment
                UInt16 adj = UBT.V2_GetAdjAngle(id);
                if (adj == 0x7F7F)
                {
                    return;
                }

                int adjValue = 0;
                if ((adj >= 0x0000) && (adj <= 0x0130))
                {
                    adjValue = adj;
                }
                else if ((adj >= 0xFED0) && (adj <= 0xFFFF))
                {
                    adjValue = (adj - 65536);
                }
                else
                {
                    UpdateInfo("Invalid adjustment", UTIL.InfoType.error);
                    return;
                }

                // get current angle
                Byte currAngle = UBT.V2_GetAngle(id);
                if (currAngle == 0xFF)
                {
                    return;
                }

                int delta = cboDelta.SelectedIndex;
                // adjValue = 3 * angle
                int newAdjValue = (currAngle - iFixAngle) * 3 + adjValue;
                if (newAdjValue < 0) newAdjValue += 65536 + delta;
                SetAdjAngle(newAdjValue);
                txtAdjPreview.Text = sFixAngle;

                SetServoAdjAngle();
            }
        }

    }
}
