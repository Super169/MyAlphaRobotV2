using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot.data
{
    public class ComboItem
    {
        public byte seqNo { get; set;}
        public string displayActionId {  get { return (repeatCount == 0 ? "" : String.Format("{0:000}", actionId));  } }
        public string displayRepeatCount { get { return (repeatCount == 0 ? "" : repeatCount.ToString()); } }

        public byte actionId = 0;
        public byte repeatCount = 0;

        public bool isEmpty { get { return (repeatCount == 0); } }

        public ComboItem(byte id)
        {
            this.seqNo = id;
            actionId = 0;
            repeatCount = 0;
        }

        public void Reset()
        {
            actionId = 0;
            repeatCount = 0;
        }

        public bool ReadFromArray(byte seqNo, byte[] data)
        {
            this.seqNo = seqNo;
            actionId = data[CONST.CI.OFFSET.COMBO_DATA + seqNo * CONST.CI.COMBO_SIZE];
            repeatCount = data[CONST.CI.OFFSET.COMBO_DATA + seqNo * CONST.CI.COMBO_SIZE + 1];
            return true;
        }
    }
}
