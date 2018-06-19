using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class ComboInfo
    {
        public byte comboId { get; set; }
        public byte actionCount { get; set; }
        public ComboItem[] comboStep = new ComboItem[CONST.CI.MAX_COMBO_SIZE];

        public ComboInfo(byte comboId)
        {
            this.comboId = comboId;
            for (byte i = 0; i < CONST.CI.MAX_COMBO_SIZE; i++)
            {
                comboStep[i] = new ComboItem(i);
            }
        }

        public void Reset()
        {
            for (byte i = 0; i < CONST.CI.MAX_COMBO_SIZE; i++)
            {
                comboStep[i].Reset();
            }
        }


        public bool ReadFromArray(byte[] data)
        {
            comboId = data[CONST.CI.OFFSET.ID];
            actionCount = 0;
            bool stepGoing = true;
            for (byte i = 0; i < CONST.CI.MAX_COMBO_SIZE; i++)
            {
                comboStep[i].ReadFromArray(i, data);
                if (stepGoing)
                {
                    if (comboStep[i].isEmpty)
                    {
                        stepGoing = false;
                    }
                    else
                    {
                        actionCount++;
                    }
                }
            }
            return true;
        }
    }
}
