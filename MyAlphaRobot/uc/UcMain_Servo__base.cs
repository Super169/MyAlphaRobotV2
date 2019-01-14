using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.uc
{
    public abstract class UcMain_Servo__base : UcMain__parent
    {
        protected int activeServo;

        public UcMain_Servo__base()
        {

        }

        public void SetActiveServo(int activeServo)
        {
            this.activeServo = activeServo;
        }


        public abstract void UpdateActiveServoInfo(byte angle);
        protected void UpdateActiveServo() { }
    }
}
