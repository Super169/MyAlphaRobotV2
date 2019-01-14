using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MyAlphaRobot
{
    /// <summary>
    /// Interaction logic for WinFirmwareUpdate.xaml
    /// </summary>
    public partial class WinFirmwareUpdate : Window
    {
        // Reference: https://github.com/espressif/esptool/wiki/Serial-Protocol

        // Supported by software loader and ROM loaders
        const byte ESP_FLASH_BEGIN = 0x02;
        const byte ESP_FLASH_DATA = 0x03;
        const byte ESP_FLASH_END = 0x04;
        const byte ESP_MEM_BEGIN = 0x05;
        const byte ESP_MEM_END = 0x06;
        const byte ESP_MEM_DATA = 0x07;
        const byte ESP_SYNC = 0x08;
        const byte ESP_WRITE_REG = 0x09;
        const byte ESP_READ_REG = 0x0A;

        // Supported by software loader and ESP32 ROM Loader
        const byte ESP_SPI_SET_PARAMS = 0x0B;
        const byte ESP_SPI_ATTACH = 0x0D;
        const byte ESP_CHANGE_BAUDRATE = 0x0F;
        const byte ESP_FLASH_DEFL_BEGIN = 0x10;
        const byte ESP_FLASH_DEFL_DATA = 0x11;
        const byte ESP_FLASH_DEFL_END = 0x12;
        const byte ESP_SPI_FLASH_MD5 = 0x13;

        // Supported by software loader only (ESP8266 & ESP32)
        const byte ESP_ERASE_FLASH = 0xD0;
        const byte ESP_ERASE_REGION = 0xD1;
        const byte ESP_READ_FLASH = 0xD2;
        const byte ESP_RUN_USER_CODE = 0xD3;

        // Checksum
        const byte ESP_CHECKSUM_SEED = 0xEF;

        // Baud rate settings
        const int ESP_BOOTLOADER_BAUD = 74800;

        // Global variables
        const int ESP_BLOCK_SIZE = 0x1000;

        // As 0xC0 & 0xDB will be changed to 0xDB 0xDC & 0xDD, this size can be more than expected block size, but will not exceed 2 X BLOCK_SIZE
        byte[] command = new byte[ESP_BLOCK_SIZE * 2];
        byte[] data = new byte[ESP_BLOCK_SIZE * 2];
        byte[] txdata = new byte[ESP_BLOCK_SIZE * 2];
        byte[] rxdata = new byte[ESP_BLOCK_SIZE * 2];

        int speed = 0;

        private void ResetSignal()
        {
            DTR_HIGH();
            RTS_HIGH();
            Thread.Sleep(1);
        }

        private void ClearBuffer()
        {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            receiveBuffer.Clear();
        }

        private bool EnterBootloader()
        {
            if (serialPort.IsOpen)
            {
                if (serialPort.BaudRate != ESP_BOOTLOADER_BAUD)
                {
                    SetBaudRate(ESP_BOOTLOADER_BAUD);
                }

            }
            else
            {
                string portName = (string)cboPorts.SelectedValue;
                if (portName == "")
                {
                    UpdateInfo("串口没选定", MyUtil.UTIL.InfoType.error);
                    return false;
                }
                if (Util.SERIAL.Connect(serialPort, portName, ESP_BOOTLOADER_BAUD))
                {
                    AppendLog(String.Format("连线到 {0}, 参数: {1}, N, 8, 1", portName, serialPort.BaudRate));
                }
            }
            if (!serialPort.IsOpen)
            {
                AppendLog("串口连线失败");
                return false;
            }
            ClearBuffer();
            RTS_LOW();
            DTR_LOW();
            Thread.Sleep(5);
            DTR_HIGH();
            RTS_HIGH();
            Thread.Sleep(2);
            DTR_LOW();
            RTS_LOW();
            Thread.Sleep(3);
            DTR_HIGH();
            Thread.Sleep(100);
            DTR_LOW();
            RTS_HIGH();
            long startTicks = DateTime.Now.Ticks;
            // wait 100 ms for reboot, in general, 60ms is enough
            long endTicks = startTicks + 100 * TimeSpan.TicksPerMillisecond;
            int lastCount = 0;
            while ((DateTime.Now.Ticks < endTicks))
            {
                if (receiveBuffer.Count > 0)
                {
                    if ((lastCount > 0) && (receiveBuffer.Count == lastCount))
                    {
                        break;
                    }
                    lastCount = receiveBuffer.Count;
                }
                // It was found that continuous byte may not be received within 1ms, so wait 2ms for safety
                Thread.Sleep(2);
            }
            long diff = (DateTime.Now.Ticks - startTicks) / TimeSpan.TicksPerMillisecond;

            if (receiveBuffer.Count > 0)
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                string sResult = ascii.GetString(receiveBuffer.ToArray());
                txtOutput.Text = string.Format("ESP 重启, 并在 {0} ms 後回传:\n{1}\n", diff, sResult);
            }
            else
            {
                AppendLog("发出启动指令没有回应");
                return false;
            }
            DTR_HIGH();
            Thread.Sleep(5);
            SetBaudRate(speed);
            Thread.Sleep(5);
            return true;
        }

        private bool Sync()
        {
            if (!serialPort.IsOpen) return false;
            ClearBuffer();
            bool success = false;

            data[0] = 0x07;
            data[1] = 0x07;
            data[2] = 0x12;
            data[3] = 0x20;
            for (int i = 4; i < 36; i++)
            {
                data[i] = 0x55;
            }
            for (int i = 0; i < 7; i++)
            {
                success = SendCommand(ESP_SYNC, data, 36, 0);
                if (success) break;
            }

            if (!success)
            {
                AppendLog("同步失败");
                return false;
            }
            AppendLog("与 ESP 同步成功");
            return true;
        }

        // Dump data array as Hex string for debugging
        private string DumpData(byte[] data, int count = 0)
        {
            if (count == 0) count = data.Length;
            if (count > data.Length) count = data.Length;
            if (count == 0) return "";

            StringBuilder hex = new StringBuilder((int)count * 3);
            for (int idx = 0; idx < count; idx++)
            {
                hex.AppendFormat(" {0:x2}", data[idx]);
            }
            return hex.ToString();
        }

        // To calculate checksum, start with seed value 0xEF and XOR each individual byte in the "data to write". 
        // The 8-bit result is stored in the checksum field of the packet header(as a little endian 32-bit value).
        private byte Checksum(int DataLen)
        {
            byte chk = ESP_CHECKSUM_SEED;
            for (int i = 0; i < DataLen; i++)
            {
                chk ^= command[i];
            }
            return chk;
        }

        // Pack the data (C0->DB DC, DB -> DB DD) and send to ESP via serial
        // 0xC0 {data} 0xC0
        private bool Write(byte[] sendData, int size, int waitMs)
        {
            int i = 0;

            txdata[i++] = 0xC0;
            for (int x = 0; x < size; x++)
            {
                if (sendData[x] == 0xC0) { txdata[i++] = 0xDB; txdata[i++] = 0xDC; }
                else if (sendData[x] == 0xDB) { txdata[i++] = 0xDB; txdata[i++] = 0xDD; }
                else { txdata[i++] = sendData[x]; }
            }
            txdata[i++] = 0xC0;
            serialPort.Write(txdata, 0, i);

            long startTicks = DateTime.Now.Ticks;
            long endTicks = DateTime.Now.Ticks + waitMs * TimeSpan.TicksPerMillisecond;
            while ((DateTime.Now.Ticks < endTicks) && (receiveBuffer.Count == 0))
            { Thread.Sleep(1); }
            return (receiveBuffer.Count > 0);
        }

        // Read data from receiveBuffer to array
        private bool Read(byte[] outBuffer, int count)
        {
            if (receiveBuffer.Count < count) return false;
            byte[] buffer = receiveBuffer.GetRange(0, count).ToArray();
            receiveBuffer.RemoveRange(0, count);
            Array.Copy(buffer, outBuffer, count);
            return true;
        }

        // Send command with provided data to ESP
        // 0x00 {cmd:1} {len:2} {checksum:4} {comData:len}
        private bool SendCommand(byte cmd, byte[] cmdData, int len, int chk, int waitMs = 50)
        {
            int i = 0;
            int retCnt = 0;

            // Command packet: 0x00 {cmd} {len:2} {checksum:4} {cmdData}
            command[i++] = 0x00;
            command[i++] = cmd;
            command[i++] = (byte)((len & 0xFF));
            command[i++] = (byte)((len >> 8) & 0xFF);
            command[i++] = (byte)((chk & 0xFF));
            command[i++] = (byte)((chk >> 8) & 0xFF);
            command[i++] = (byte)((chk >> 16) & 0xFF);
            command[i++] = (byte)((chk >> 24) & 0xFF);
            for (int idx = 0; idx < len; idx++)
            {
                command[i++] = cmdData[idx];
            }

            if (!Write(command, i, waitMs))
            {
                errmsg = "发送资料後没有回传";
                return false;
            }

            byte[] fullResult = receiveBuffer.ToArray();
            Read(command, 1);
            if (command[0] != 0xC0)
            {
                errmsg = "回传首码异常 " + DumpData(fullResult);
                return false;
            }

            Read(command, 8);
            if (command[0] != 0x01 || (cmd != command[1]))
            {
                errmsg = "回传数据异常" + DumpData(fullResult);
                return false;
            }

            retCnt = command[2] + command[3] * 256;
            Read(command, retCnt);
            if (retCnt != 2 || command[0] != 0x00 || command[1] != 0x00) // Something bad happened
            {
                AppendLog("指令失败: " + retCnt + " 位数据包为: " + DumpData(command, retCnt));
                errmsg = "指令失败, 固传讯息错误: " + DumpData(fullResult);
                return false;
            }

            Read(command, 1);
            if (command[0] != 0xC0)
            {
                errmsg = "回传结束码异常: " + DumpData(fullResult);
                return false;
            }
            return true;
        }

        // FLASH BEGIN: 16 bytes data packet
        // {data size:4} {packet count:4} {packet size:4} {offset: 4}
        private bool FLASH_BEGIN(int dataSize, int noPacket, int packetSize, int offset)
        {
            ClearBuffer();
            int i = 0;

            data[i++] = (byte)((dataSize & 0xFF));
            data[i++] = (byte)((dataSize >> 8) & 0xFF);
            data[i++] = (byte)((dataSize >> 16) & 0xFF);
            data[i++] = (byte)((dataSize >> 24) & 0xFF);
            data[i++] = (byte)((noPacket & 0xFF));
            data[i++] = (byte)((noPacket >> 8) & 0xFF);
            data[i++] = (byte)((noPacket >> 16) & 0xFF);
            data[i++] = (byte)((noPacket >> 24) & 0xFF);
            data[i++] = (byte)((packetSize & 0xFF));
            data[i++] = (byte)((packetSize >> 8) & 0xFF);
            data[i++] = (byte)((packetSize >> 16) & 0xFF);
            data[i++] = (byte)((packetSize >> 24) & 0xFF);
            data[i++] = (byte)((offset & 0xFF));
            data[i++] = (byte)((offset >> 8) & 0xFF);
            data[i++] = (byte)((offset >> 16) & 0xFF);
            data[i++] = (byte)((offset >> 24) & 0xFF);
            return SendCommand(ESP_FLASH_BEGIN, data, i, 0, 10000);
        }

        // FLASH BEGIN: n bytes data packet
        // {data size:4} {seq:4} {data:??}
        private bool FLASH_DATA(int DataLen, int Sequence)
        {
            ClearBuffer();

            int i = 0;

            data[i++] = (byte)((DataLen & 0xFF));
            data[i++] = (byte)((DataLen >> 8) & 0xFF);
            data[i++] = (byte)((DataLen >> 16) & 0xFF);
            data[i++] = (byte)((DataLen >> 24) & 0xFF);
            data[i++] = (byte)((Sequence & 0xFF));
            data[i++] = (byte)((Sequence >> 8) & 0xFF);
            data[i++] = (byte)((Sequence >> 16) & 0xFF);
            data[i++] = (byte)((Sequence >> 24) & 0xFF);
            data[i++] = 0x00;
            data[i++] = 0x00;
            data[i++] = 0x00;
            data[i++] = 0x00;
            data[i++] = 0x00;
            data[i++] = 0x00;
            data[i++] = 0x00;
            data[i++] = 0x00;
            for (int x = 0; x < DataLen; x++)
            {
                data[i++] = command[x];
            }
            return SendCommand(ESP_FLASH_DATA, data, i, (int)(Checksum(DataLen)), 1000);
        }

        // FLASH BEGIN: 4 bytes data packet
        // {reboot:4} 
        private bool FLASH_END(bool reboot)
        {
            ClearBuffer();
            int i = 0;

            data[i++] = (byte)(reboot ? 0x01 : 0x00);
            data[i++] = 0;
            data[i++] = 0;
            data[i++] = 0;
            return SendCommand(ESP_FLASH_END, data, i, 0, 1000);
        }


    }
}
