using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot
{
    class Stm8SendData
    {
        Byte startPage = 0x09;
        List<Stm8HexData> data = new List<Stm8HexData>();
        int dataRowCnt = 0;

        public Byte StartPage { get { return startPage; } }
        public int DataRowCount { get { return dataRowCnt; } }
        public Byte EndPage { get { return (Byte)(startPage + data.Count - 1); } }
        public UInt16 StartAddr { get { return (UInt16)(0x8000 + startPage * 0x40); } }

        public Stm8HexData GetHexData(int idx)
        {
            if ((idx < 0) || (idx >= data.Count)) return null;
            return data[idx];

        }

        public int Count { get { return data.Count; } }

        public bool ReadFile(List<string> txtData)
        {
            data = new List<Stm8HexData>();
            if (txtData.Count == 0) return true;

            string r1 = txtData[0];
            int page, offset, dataLen;
            if (!Stm8HexData.GetPage(r1, out page, out offset, out dataLen)) return false;
            if (offset != 0) return false;

            startPage = (byte)page;

            dataRowCnt = 0;
            int line = 0;
            int cmdPage = startPage;
            bool goNext = true;
            while (goNext && (line < txtData.Count))
            {
                Stm8HexData hd = new Stm8HexData(cmdPage);
                int dataRow = 0;
                if (hd.ReadData(line, txtData, out dataRow))
                {
                    dataRowCnt += dataRow;
                    data.Add(hd);
                }
                else
                {
                    goNext = false;
                    break;
                }
                cmdPage++;
                line += 4;
            }
            return true;
        }

    }
}
