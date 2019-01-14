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
    /// Interaction logic for UcMain_Servo_HaiLzd.xaml
    /// </summary>
    public partial class UcMain_Servo_HaiLzd : UcMain_Servo__base
    {
        public UcMain_Servo_HaiLzd()
        {
            InitializeComponent();
        }

        public override void UpdateActiveServoInfo(byte angle)
        {
            bool activeReady = (activeServo > 0);
            tbActiveAdjServoId.Text = (activeReady ? activeServo.ToString() : "");
            gridServoFix.IsEnabled = activeReady;
        }

        private void btnResetServo_Click(object sender, RoutedEventArgs e)
        {
            if (activeServo <= 0) return;
            UBT.V2_ServoCommand((byte)activeServo, 1);
        }


        private void btnAutoFixAngle_Click(object sender, RoutedEventArgs e)
        {
            int angle = 0;
            try
            {
                angle = int.Parse(txtFixAngle.Text.Trim());
                if ((angle < 0) || (angle > 180))
                {
                    UpdateInfo("輸入角度不正確", MyUtil.UTIL.InfoType.error);
                    return;
                }
            }
            catch (Exception)
            {
                UpdateInfo("輸入角度不正確", MyUtil.UTIL.InfoType.error);
                return;
            }

            byte id = (byte)activeServo;
            if (UBT.V2_SetAngle(id, (byte) angle, 0))
            {
                UBT.V2_MoveServo(id, (byte) angle, 50);
            }
            UBT.LockServo(id, true);
            UpdateActiveServo();
        }
    }
}

