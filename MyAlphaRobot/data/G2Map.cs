using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class G2Map
    {

        public bool isReady { get; }
        int[] _dir = new int[CONST.MAX_SERVO + 1];
        byte[] _min = new byte[CONST.MAX_SERVO + 1];
        byte[] _max = new byte[CONST.MAX_SERVO + 1];
        byte[] _g1Ref = new byte[CONST.MAX_SERVO + 1];
        byte[] _g2Ref = new byte[CONST.MAX_SERVO + 1];

        public G2Map(string[] csv)
        {
            isReady = false;
            if (csv.Length < 6) return;

            if (!GetDataInt(_dir, csv[1])) return;
            if (!GetDataByte(_min, csv[2])) return;
            if (!GetDataByte(_max, csv[3])) return;
            if (!GetDataByte(_g2Ref, csv[4])) return;
            if (!GetDataByte(_g1Ref, csv[5])) return;

            if (!VerifyData()) return;

            isReady = true;
        }

        private bool GetDataInt(int[] result, string data)
        {
            string[] sValue = data.Split(',');
            int fillMax = Math.Min(result.Length, sValue.Length);
            for (int id = 1; id < fillMax; id++)
            {
                int value;
                if (!int.TryParse(sValue[id], out value))
                {
                    return false;
                }
                result[id] = value;
            }
            return true;
        }

        private bool GetDataByte(byte[] result, string data)
        {
            string[] sValue = data.Split(',');
            int fillMax = Math.Min(result.Length, sValue.Length);
            for (int id = 1; id < fillMax; id++)
            {
                byte value;
                if (!byte.TryParse(sValue[id], out value))
                {
                    return false;
                }
                result[id] = value;
            }
            return true;
        }

        private bool VerifyData()
        {
            for (int id = 1; id <= 16; id++)
            {
                if ((_dir[id] != 1) && (_dir[id] != -1)) return false;
                if (_min[id] > _max[id])
                {
                    byte _tmp = _min[id];
                    _min[id] = _max[id];
                    _max[id] = _tmp;
                }
                if (_max[id] > CONST.SERVO.MAX_ANGLE) return false;
                if (_g2Ref[id] > CONST.SERVO.MAX_ANGLE) return false;
                if (_g1Ref[id] > CONST.SERVO.MAX_ANGLE) return false;
            }
            return true;
        }

        public byte Convert(byte id, byte angle)
        {
            if (id > CONST.MAX_SERVO) return 255;
            int newAngle = _g2Ref[id] + (angle - _g1Ref[id]) * _dir[id];
            if (newAngle < 0) return _min[id];
            if (newAngle < _min[id]) return _min[id];
            if (newAngle > _max[id]) return _max[id];
            return (byte)newAngle;
        }
    }
}
