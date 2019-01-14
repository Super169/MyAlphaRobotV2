using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        DispatcherTimer sliderTimer = new DispatcherTimer();
        DispatcherTimer backgroundTimer = new DispatcherTimer();
        private delegate void SerialTimerHandler();
        private int sliderMode = 0;

        // reduce calculation, put the next ms for comparison
        private long nextBatteryTicks = 0;
        private long nextMpuTicks = 0;

        private const int batteryUpdateSec = 10;
        private const int mpuUpdateSec = 1; 

        private void InitTimer()
        {
            sliderTimer.Tick += new EventHandler(sliderTimer_Tick);
            sliderTimer.Interval = TimeSpan.FromMilliseconds(20);
            sliderTimer.Stop();

            // If last move not completed, check every second
            backgroundTimer.Tick += new EventHandler(backgroundTimer_Tick);
            backgroundTimer.Interval = TimeSpan.FromSeconds(1);
            backgroundTimer.Stop();
        }

        private void sliderTimer_Tick(object sender, EventArgs e)
        {
            sliderTimer.Stop();


            double dblAngle = Math.Round(sliderActiveAngle.Value);
            byte angle = (byte) dblAngle;
            UBT.MoveServo((byte)activeServo, angle, 0);
            if (sliderMode > 0)
            {
                // extra one time after stop
                if (sliderMode == 1) sliderMode = 0;
                sliderTimer.Start();
            }
        }

        // Try to put all background communization in single routine
        private void backgroundTimer_Tick(object sender, EventArgs e)
        {
            backgroundTimer.Stop();
            if (!systemWorking)
            {
                backgroundRunning = true;
                Thread.Sleep(1);

                if (!SYSTEM.sc.disableBatteryUpdate)
                {
                    if (DateTime.Now.Ticks > nextBatteryTicks)
                    {
                        if (!systemWorking)
                        {
                            UpdateBattery();
                        }
                        nextBatteryTicks = DateTime.Now.Ticks + batteryUpdateSec * TimeSpan.TicksPerSecond;
                    }
                }

                if (!SYSTEM.sc.disableMpuUpdate)
                {
                    if (DateTime.Now.Ticks > nextMpuTicks)
                    {
                        if (!systemWorking)
                        {
                            UpdateMpu();
                        }
                        nextMpuTicks = DateTime.Now.Ticks + mpuUpdateSec * TimeSpan.TicksPerSecond;
                    }

                }
            }
            backgroundRunning = false;
            backgroundTimer.Start();
        }

        public void ClearBattery()
        {
            UIUpdateBattery(0, 0);
        }

        public void UpdateBattery()
        {

            // Just for safety, in case front-end submit request at the same time
            UInt16 value;
            byte power;
            UBT.CheckBattery(out value, out power);
            UIUpdateBattery(value, power);
        }

        public void UIUpdateBattery(UInt16 value, byte power)
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                Application.Current.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  (Action)(() => UIUpdateBattery(value, power)));
                return;
            }
            if (power > 70)
            {
                lblBattery.Foreground = Brushes.GreenYellow;
                lblBatteryValue.Foreground = Brushes.GreenYellow;
            }
            else if (power > 30)
            {
                lblBattery.Foreground = Brushes.Yellow;
                lblBatteryValue.Foreground = Brushes.Yellow;
            }
            else
            {
                lblBattery.Foreground = Brushes.Red;
                lblBatteryValue.Foreground = Brushes.Red;
            }
            if (power > 100) power = 100;
            if (value == 0)
            {
                lblBattery.Content = "";
                lblBatteryValue.Content = "";
            } else
            {
                lblBattery.Content = String.Format("{0}%", power);
                lblBatteryValue.Content = value.ToString();
            }

        }

        public void ClearMpu()
        {
            UIUdateMpu();
        }

        public void UpdateMpu()
        {
            if (!UBT.mpuExists) return;
            Int16 ax, ay, az;
            if (UBT.GetMpuData(out ax, out ay, out az))
            {
                UIUdateMpu(ax.ToString(), ay.ToString(), az.ToString());
            } else
            {
                UIUdateMpu();
            }
        }

        public void UIUdateMpu(string ax = "", string ay = "", string az = "")
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                Application.Current.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  (Action)(() => UIUdateMpu(ax, ay, az)));
                return;
            }
            lbl_ax.Content = ax;
            lbl_ay.Content = ay;
            lbl_az.Content = az;
        }


        public void StartSystemWork(bool updateUI = true)
        {
            if (updateUI) Mouse.OverrideCursor = Cursors.Wait;
            while (backgroundRunning)
            {
                Thread.Sleep(1);
            }
            systemWorking = true;
        }

        public void EndSystemWork(bool updateUI = true)
        {
            systemWorking = false;
            if (updateUI) Mouse.OverrideCursor = null;
        }

    }
}
