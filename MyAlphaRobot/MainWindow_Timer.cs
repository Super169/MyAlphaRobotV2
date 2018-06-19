using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        DispatcherTimer sliderTimer = new DispatcherTimer();
        DispatcherTimer servoTimer = new DispatcherTimer();
        DispatcherTimer batteryTimer = new DispatcherTimer();
        private delegate void SerialTimerHandler();
        private long servoEndTicks = 0;

        private void InitTimer()
        {
            sliderTimer.Tick += new EventHandler(sliderTimer_Tick);
            // prevent frequent update, hold for 500s before udpate servo
            sliderTimer.Interval = TimeSpan.FromMilliseconds(5);
            sliderTimer.Stop();

            // If last move not completed, check every 10ms
            servoTimer.Tick += new EventHandler(servoTimer_Tick);
            servoTimer.Interval = TimeSpan.FromMilliseconds(5);
            servoTimer.Stop();

            // If last move not completed, check every 10ms
            batteryTimer.Tick += new EventHandler(batteryTimer_Tick);
            batteryTimer.Interval = TimeSpan.FromSeconds(10);
            batteryTimer.Stop();
        }


        private void servoTimer_Tick(object sender, EventArgs e)
        {
            servoTimer.Stop();
            if (DateTime.Now.Ticks >= servoEndTicks)
            {
                sliderTimer.Stop();
                sliderTimer.Start();
            } else
            {
                servoTimer.Start();
            }
        }

        private void sliderTimer_Tick(object sender, EventArgs e)
        {
            if (activeServo > 0)
            {
                sliderTimer.Stop();
                double dblAngle = Math.Round(sliderActiveAngle.Value);
                byte angle = (byte) dblAngle;
                byte oldAngle = servo[activeServo].angle;
                // if (angle == oldAngle) return;
                double diff = (byte) Math.Abs(angle - oldAngle);
                byte time = (byte) (Math.Round(40.0 * diff / CONST.SERVO.MAX_ANGLE));  // 40 ~ 1s, 1s for 240 angle
                UBT.MoveServo((byte)activeServo, angle, time);
                int TimeMs = time * 25 + 100;    // add extra 100ms, seeem still not work, sometimes command cannot be executed for fast change.
                servoEndTicks = DateTime.Now.Ticks + TimeMs * TimeSpan.TicksPerMillisecond;
            }
        }

        private void batteryTimer_Tick(object sender, EventArgs e)
        {
            batteryTimer.Stop();
            if (!systemWorking)
            {
                batteryUpdating = true;
                Thread.Sleep(1);
                if (!systemWorking)
                {
                    UpdateBattery();
                }
            }
            batteryUpdating = false;
            batteryTimer.Start();
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
                lblBattery.Content = "---";
                lblBatteryValue.Content = "---";
            } else
            {
                lblBattery.Content = String.Format("{0}%", power);
                lblBatteryValue.Content = value.ToString();
            }

        }

        public void StartSystemWork()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            while (batteryUpdating)
            {
                Thread.Sleep(1);
            }
            systemWorking = true;
        }

        public void EndSystemWork()
        {
            systemWorking = false;
            Mouse.OverrideCursor = null;
        }

    }
}
