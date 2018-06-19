using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace MyAlphaRobot
{
    /// <summary>
    /// Interaction logic for WinStm8Writer.xaml
    /// </summary>
    public partial class WinStm8Writer : Window
    {
        private StringBuilder sbLog = new StringBuilder();
        Stm8SendData sendData = new Stm8SendData();
        bool hexFileReady = false;

        Thread TBurn = null;

        private void UpdateProgress(string msg)
        {
            /*
            this.txtOutput.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                (Action)(() => { txtOutput.Text += msg; txtOutput.ScrollToEnd(); }));
            */
        }

        private void UpdatePorgress(double progress)
        {
            this.pbProgress.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                (Action)(() => { pbProgress.Value = progress; }));
        }

        private void UpdateLog(string msg, bool updateProgress = false)
        {
            sbLog.Append(msg);
            if (updateProgress) UpdateProgress(msg);
        }

        private void btnCheckLog_Click(object sender, RoutedEventArgs e)
        {
            if ((TBurn == null) || (sbLog.Length == 0))
            {
                UpdateInfo("沒有燒錄資料");
                return;
            }
            if (TBurn.ThreadState != ThreadState.Stopped)
            {
                UpdateInfo("燒錄進行中, 暫時不能查看");
                return;
            }
        }

        private void btnBurn_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            if (TBurn != null)
            {
                if (TBurn.ThreadState != ThreadState.Stopped)
                {
                    UpdateInfo("Burding in action....", UTIL.InfoType.error);
                    return;
                }
                TBurn = null;
            }
            TBurn = new Thread(GoBurnThread);
            sbLog = new StringBuilder();
            UpdatePorgress(0);
            TBurn.Start();
        }

        private void GoBurnThread()
        {
            SetButtons(false);
            GoBurn();
            SetButtons(true);
        }


        private void SetButtons(bool status)
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                Application.Current.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (Action)(() => SetButtons(status)));
                return;
            }
            btnConnect.IsEnabled = status;
            btnLoadFile.IsEnabled = status;
            btnBurn.IsEnabled = status;
            btnClose.IsEnabled = true;
        }




        private void GoBurn()
        {
            UpdateInfo("偵測舵機");
            UpdateLog("偵測舵機.\r\n=> ", true);
            bool waitReply = true;
            int sendCnt = 0;
            int maxSend = 500;
            byte lastByte = 0;
            while (waitReply)
            {
                if (sendCnt > maxSend)
                {
                    UpdateLog("\r\n\r\n偵測超時: 舵機沒回應", true);
                    UpdateInfo("偵測超時: 舵機沒回應", UTIL.InfoType.error);
                    return;
                }

                SerialSendByte(0xA5, true);
                Thread.Sleep(10);
                sendCnt++;

                while (receiveBuffer.Count > 0)
                {
                    UpdateLog("\r\n<= ", true);
                    lastByte = receiveBuffer[0];
                    UpdateLog(String.Format(" {0:X2}", lastByte), true);
                    // Just for safety, differnt response from servo, A0 & A1 may cause by previous incomplete write
                    if ((lastByte == 0xA9) || (lastByte == 0xE9) || (lastByte == 0xA1) || (lastByte == 0xA0))
                    {
                        waitReply = false;
                    }
                    receiveBuffer.RemoveAt(0);
                }

            }

            if ((lastByte == 0xA0) || (lastByte == 0xA1))
            {
                string s1 = "偵測到之前燒錄未完成";
                string s2 = "安全起見, 請重置舵機, 再次重新燒錄.";

                UpdateLog(string.Format("\r\n\r\n!!!! {0} !!!!\r\n{1}", s1, s2), true);
                MessageBox.Show(s2, s1);
                return;
            }
            UpdateInfo("燒錄進行中, 請耐心等待......");
            UpdateLog("\r\n\r\n開始燒錄......\r\n", true);
            double nPage = sendData.Count;
            byte endPage = sendData.EndPage;
            if (endPage < 0x7F)
            {
                nPage += (0x7F - endPage);
            }
            for (int i = 0; i < sendData.Count; i++)
            {
                Stm8HexData hd = sendData.GetHexData(i);
                if (!SendHexDate(hd, i)) return;
                UpdatePorgress((double)100.0 * i / nPage);
            }
            for (int i = endPage + 1; i < 0x80; i++)
            {
                Stm8HexData hd = new Stm8HexData(i);
                hd.Reset();
                if (!SendHexDate(hd, 999)) return;
                UpdatePorgress((double)100.0 * (sendData.Count + i) / nPage);
            }
            UpdatePorgress(100);

            UpdateLog("\r\n=> ");
            SerialSendByte(0xA9);
            if (WaitServoReturn(out lastByte))
            {
                if (lastByte == 0xA0)
                {
                    UpdateLog("\r\n\r\n燒錄完成\r\n", true);
                    UpdateInfo("燒錄順利完成");
                    return;
                }
            }

            UpdateLog("\r\n\r\n燒錄完成, 但終結的 0xA9 沒回傳 A0\r\n", true);
            UpdateInfo("燒錄完成, 但終結的 0xA9 沒回傳");
            return;

        }

        private bool SendHexDate(Stm8HexData hd, int row)
        {
            int retryCnt = 0;
            while (retryCnt < 5)
            {
                UpdateProgress(String.Format(" {0:X2}:", hd.Page));
                if (hd == null)
                {
                    UpdateLog(String.Format("\r\n\r\n第 {0} 筆資料錯誤, 燒錄停止......\r\n", row));
                    return false;
                }
                else
                {
                    byte servoResponse;
                    if (!WriteStm8HexData(hd, out servoResponse))
                    {
                        UpdateLog("\r\n\r\n燒錄失敗......\r\n", true);
                        return false;
                    }
                    if (servoResponse == 0xA0) break;
                    if (servoResponse != 0xA1)
                    {
                        UpdateLog("\r\n\r\n回傳不正常, 燒錄停止......\r\n", true);
                        return false;
                    }
                }
                UpdateProgress("ERR ");
                retryCnt++;
            }
            if (retryCnt >= 5)
            {
                UpdateLog(String.Format("\r\n\r\n第 {0} 筆資料多次傳送錯誤, 燒錄停止......\r\n", row));
                return false;
            }
            UpdateProgress("OK ");
            return true;
        }

        private bool WriteStm8HexData(Stm8HexData hd, out byte servoReturn)
        {
            UpdateLog("\r\n=> ");
            receiveBuffer.Clear();
            byte[] startCode = { 0xA7, hd.Page };
            SerialSendByte(startCode);
            if (receiveBuffer.Count > 0)
            {
                // unexpected return;
                byte[] b = ShowReceiveBuffer();
                servoReturn = b[b.Length - 1];  // just check the last one is enqugh
                return false;
            }
            for (int i = 0; i < 64; i++)
            {
                SerialSendByte(hd.Data(i));
                if (ServoReturned(out servoReturn)) return false;
            }
            SerialSendByte(hd.CheckSum);
            return WaitServoReturn(out servoReturn);
        }

        private bool WriteEmptyPage(byte page, out byte servoReturn)
        {
            Stm8HexData hd = new Stm8HexData(page);
            return WriteStm8HexData(hd, out servoReturn);
        }

        private bool WaitServoReturn(out byte lastByte)
        {
            int cnt = 0;
            lastByte = 0x00;
            while (cnt < 100)
            {
                Thread.Sleep(10);
                if (receiveBuffer.Count > 0)
                {
                    byte[] b = ShowReceiveBuffer();
                    lastByte = b[b.Length - 1];
                    return true;
                }
                UpdateLog(" .");
                cnt++;
            }
            SerialSendByte(0xA5);
            Thread.Sleep(100);
            if (receiveBuffer.Count > 0)
            {
                byte[] b = ShowReceiveBuffer();
                // lastByte = b[b.Length-1];
                lastByte = 0xA1;  // Treat as fail, and force retry
                return true;
            }
            UpdateLog("\r\n操作超時: 舵機沒回應");
            return false;
        }

        private bool ServoReturned(out byte lastByte)
        {
            lastByte = 0;
            if (receiveBuffer.Count == 0) return false;

            byte[] b = ShowReceiveBuffer();
            // just check last byte is enough, keep the array in case more checking later
            lastByte = b[b.Length - 1];
            return true;
        }

        private byte[] ShowReceiveBuffer()
        {
            byte[] b = receiveBuffer.ToArray();
            receiveBuffer.Clear();
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<= ");
            for (int i = 0; i < b.Length; i++)
            {
                sb.Append(String.Format(" {0:X2}", b[i]));
            }
            UpdateLog(sb.ToString());
            return b;
        }


    }
}
