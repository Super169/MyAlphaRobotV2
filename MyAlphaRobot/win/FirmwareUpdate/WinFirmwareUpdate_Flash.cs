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
        Thread TBurn = null;
        private StringBuilder sbLog = new StringBuilder();
        string filename;
        string errmsg = "";
        byte[] firmware;

        private void FlashFirmware()
        {
            UpdateInfo();
            if (BurnInProgress())
            {
                MessageBox.Show("烧录进行中....请耐心等候");
                return;
            }

            string sSpeed = cboSpeed.Text;
            if (!int.TryParse(sSpeed, out speed) || (speed < 9600) || (speed > 921600))
            {
                MessageBox.Show("输入的速度不正常, 请输入 9600 - 921600 之间的速度");
                return;
            }

            if (!EnterBootloader())
            {
                ResetSignal();
                return;
            }

            if (!Sync())
            {
                return;
            }

            TBurn = new Thread(GoBurnThread);
            SetActionGrid(false);
            TBurn.Start();
        }



        private void GoBurnThread()
        {
            string msg = "固件烧录";
            long startTicks = DateTime.Now.Ticks;
            if (FlashFirmware(0, firmware))
            {
                long endTicks = DateTime.Now.Ticks;
                long diffMs = (endTicks - startTicks) / TimeSpan.TicksPerMillisecond;
                msg += string.Format("成功, 需时 {0:n0} ms", diffMs);
                UpdateInfo(msg);
                MyUtil.UTIL.WriteRegistry(MyUtil.UTIL.KEY.LAST_CONNECTION_SERIAL, serialPort.PortName);
            }
            else
            {
                msg += "失败: ";
                UpdateInfo(msg);
                msg += errmsg;
            }
            AppendLog(msg);
            AppendOutput(msg);
            serialPort.Close();
            SetActionGrid(true);
        }


        private bool FlashFirmware(int offset, byte[] flashdata)
        {

            int blocks = 0;
            int remainBytes = 0;
            errmsg = "";
            blocks = (int)(Math.Ceiling((double)flashdata.Length / (double)(ESP_BLOCK_SIZE)));
            AppendLog("开始烧录固件");
            AppendLog(string.Format("File size: {0:n0} bytes, block size: {1:n0} bytes, total {2:n0} blocks, starting from 0x{3:x08}", flashdata.Length, ESP_BLOCK_SIZE, blocks, offset));

            if (!FLASH_BEGIN(flashdata.Length, blocks, ESP_BLOCK_SIZE, offset))
            {
                errmsg = "FLASH_BEGIN: " + errmsg;
                return false;
            }

            for (int seq = 0; seq < blocks; seq++)
            {
                Thread.Sleep(1);
                // AppendLog("Sending chunk [" + (seq + 1) + "/" + blocks + "]");
                remainBytes = flashdata.Length - (seq * ESP_BLOCK_SIZE);

                if (remainBytes > ESP_BLOCK_SIZE)
                {
                    Array.Copy(flashdata, seq * ESP_BLOCK_SIZE, command, 0, ESP_BLOCK_SIZE);
                    if (!FLASH_DATA(ESP_BLOCK_SIZE, seq))
                    {
                        errmsg = string.Format("FLASH_DATA({0}) : ", seq) + errmsg;
                        return false;
                    }
                }
                else
                {
                    Array.Copy(flashdata, seq * ESP_BLOCK_SIZE, command, 0, remainBytes);
                    if (!FLASH_DATA(remainBytes, seq))
                    {
                        errmsg = string.Format("FLASH_DATA({0}) : ", seq) + errmsg;
                        return false;
                    }
                }
                UpdateProgress(100 * (seq + 1) / blocks);
            }

            if (!FLASH_END(true))
            {
                errmsg = "FLASH_END: " + errmsg;
                return false;
            }
            return true;
        }
    }
}
