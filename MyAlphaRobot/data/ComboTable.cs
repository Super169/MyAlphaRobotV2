using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class ComboTable
    {
        public ComboInfo[] combo = new ComboInfo[CONST.CI.MAX_COMBO];

        public ComboTable()
        {
            for (byte i = 0; i < CONST.CI.MAX_COMBO; i++) combo[i] = new ComboInfo(i);
        }

        public void Reset()
        {
            for (byte i = 0; i < CONST.CI.MAX_COMBO; i++)
            {
                combo[i].Reset();
            }
        }

        public bool ReadFromArray(byte cId, byte[] data)
        {
            if (cId >= CONST.CI.MAX_COMBO) return false;
            if (cId != data[CONST.CI.OFFSET.ID]) return false;
            combo[cId].ReadFromArray(data);
            return true;
        }

    }
}
