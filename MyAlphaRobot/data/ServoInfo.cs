using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class ServoInfo
    {
        public byte id;
        public bool exists;
        public bool locked;
        public byte angle;
        public byte led;
        // 00 - no action
        // 03 - turn off (* 03 = 11b, UBT command, 1 = off)
        // 02 - turn on  ** 02 = 10b, UBT command, 0 = on)

        public ServoInfo(byte id)
        {
            this.id = id;
            this.exists = false;
            this.locked = false;
            this.angle = 0xff;
            this.led = 0x00;
        }

        public ServoInfo(byte id, byte angle, byte led = 0x00)
        {
            this.id = id;
            this.exists = (angle <= CONST.SERVO.MAX_ANGLE);
            if (this.exists)
            {
                this.angle = angle;
                this.led = led;
            } else {
                this.angle = 0xff;
                this.led = 0x00;
            }
            this.locked = false;
        }




        public void Reset()
        {
            this.exists = false;
            this.locked = false;
            this.angle = 0xff;
        }

        public void Update(bool exists, byte angle, bool locked)
        {
            if ((exists) && (angle != 0xFF))
            {
                this.exists = true;
                this.angle = angle;
                this.locked = locked;
            } else
            {
                this.exists = false;
                this.angle = 0xFF;
                this.locked = false;
            }
        }

    }
}
