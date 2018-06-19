using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAlphaRobot
{
    class Stm8HexData
    {
        int pageNo;
        public byte[] data = new Byte[64];
        byte checkSum = 0;

        public byte Page { get { return (byte)pageNo; } }

        public byte Data(int idx)
        {
            return data[idx];
        }

        public byte CheckSum { get { return checkSum; } }

        public Stm8HexData(int pageNo)
        {
            this.pageNo = pageNo;
        }

        public void Reset()
        {
            for (int i = 0; i < 64; i++) data[i] = 0;
            checkSum = 0;
        }

        public bool ReadData(int startRow, List<string> txtData, out int dataRow)
        {
            dataRow = 4;
            for (int i = 0; i < 4; i++)
            {
                // dataRow == 4 means not yet finished
                if ((startRow + i < txtData.Count) && (dataRow == 4))
                {
                    string row = txtData[startRow + i];
                    if (!UpdatDataRow(row))
                    {
                        if (i == 0) return false;
                        // if not the first row, continue to fill with 0
                        dataRow = i;
                    }
                }

                // dataRow != 4 mean finished
                if ((startRow + i >= txtData.Count) || (dataRow != 4))
                {
                    // Fill zero
                    for (int j = 0; j < 16; j++)
                    {
                        data[16 * i + j] = 0x00;
                    }
                }
            }
            int sum = 0;
            for (int i = 0; i < 64; i++)
            {
                sum += data[i];
            }
            checkSum = (byte)(sum & 0xFF);

            return true;
        }

        public bool UpdatDataRow(string row)
        {
            int page, offset, dataLen;
            if (!GetPage(row, out page, out offset, out dataLen)) return false;
            if (page != this.pageNo) return false;
            int rowSum = 0;
            for (int i = 0; i < 4; i++)
            {
                byte b = Convert.ToByte(row.Substring(1 + 2 * i, 2), 16);
                rowSum += b;
            }
            for (int i = 0; i < 16; i++)
            {
                byte b = (byte)(i < dataLen ? Convert.ToByte(row.Substring(9 + 2 * i, 2), 16) : 0x00);
                data[16 * offset + i] = b;
                rowSum += b;
            }
            byte checkSum = Convert.ToByte(row.Substring(dataLen * 2 + 9, 2), 16);
            rowSum = ((256 - (rowSum & 0xFF)) & 0xFF);
            if (rowSum != checkSum) return false;
            this.checkSum = (byte)((this.checkSum + rowSum) & 0xFF);
            return true;
        }

        public static bool GetPage(string row, out int page, out int offset, out int dataLen)
        {
            page = -1;
            offset = -1;
            dataLen = -1;
            // if (row.Length != 43) return false;
            if (!row.StartsWith(":")) return false;
            // at least 11 bytes even length = 0 :LLAAAATTCC, for safetyp in future checking
            if (row.Length < 11) return false;
            dataLen = Convert.ToInt16(row.Substring(1, 2), 16);
            if (row.Length != dataLen * 2 + 11) return false;
            if (GetRecordType(row) != DataType.Data) return false;
            UInt16 addr;
            try
            {
                addr = Convert.ToUInt16(row.Substring(3, 4), 16);
            }
            catch
            {
                return false;
            }
            page = ((addr - 0x8000) / 64);
            offset = (((addr - 0x8000) / 16) % 4);
            if ((page < 0) || (page > 0xFF)) return false;
            return true;
        }

        public enum DataType
        {
            Data = 0x00, EOF = 0x01, ESAR = 0x02, SSAR = 0x03, ELAR = 0x04, SLAR = 0x05, UNKNOWN = 0xFF
        }

        public static DataType GetRecordType(string row)
        {
            Int32 b = Convert.ToByte(row.Substring(7, 2), 16);
            if (Enum.IsDefined(typeof(DataType), b))
            {
                return (DataType)b;
            }
            return DataType.UNKNOWN;
        }

    }
}
