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
    /// Interaction logic for UcMain_Servo.xaml
    /// </summary>
    public partial class UcMain_Servo : UcMain__parent
    {
        private byte servoType = 0;
        private UcMain_Servo__base ucServo;
        private int activeServo;

        public UcMain_Servo()
        {
            InitializeComponent();
        }

        public void CheckServoType()
        {
            this.gridMain.Children.Clear();
            servoType = UBT.GetServoType();
            switch (servoType)
            {
                case CONST.SERVO_TYPE.UBT:
                    {
                        ucServo = new UcMain_Servo_UBT();
                        ucServo.InitObject(UBT, updateInfo, commandHandler);
                        this.gridMain.Children.Add(ucServo);
                        break;
                    }
                case CONST.SERVO_TYPE.HaiLzd:
                    {
                        ucServo = new UcMain_Servo_HaiLzd();
                        ucServo.InitObject(UBT, updateInfo, commandHandler);
                        this.gridMain.Children.Add(ucServo);
                        break;
                    }
            }

        }

        public void SetActiveServo(int activeServo)
        {
            this.activeServo = activeServo;
            if (ucServo != null) ucServo.SetActiveServo(activeServo);
        }

        public void UpdateActiveServoInfo(byte angle)
        {
            if (ucServo != null) ucServo.UpdateActiveServoInfo(angle);

        }

    }
}
