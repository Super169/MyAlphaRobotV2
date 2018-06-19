using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyAlphaRobot.uc
{
    /// <summary>
    /// Interaction logic for UcServo.xaml
    /// </summary>
    public partial class UcServo : UserControl
    {
        Brush UNAVAILABLE_BRUSH = new SolidColorBrush(Colors.Gray);
        Brush LOCKED_BRUSH = new SolidColorBrush(Colors.Red);
        Brush UNLOCK_BRUSH = new SolidColorBrush(Colors.LightGreen);

        const int NORMAL_THICKNESS = 1;
        Brush NORMAL_STROKE = new SolidColorBrush(Colors.Black);
        const int SELECTED_THICKNESS = 4;
        Brush SELECTED_STROKE = new SolidColorBrush(Colors.Blue);

        Brush LED_ON = new SolidColorBrush(Colors.YellowGreen);
        Brush LED_OFF = new SolidColorBrush(Colors.DarkSlateGray);

        public event EventHandler MouseDownEventHandler;

        public bool isSelected = false;
        public bool isActive = false;
        public data.ServoInfo si;

        public byte id { get { return si.id; } }
        public bool exists { get { return si.exists; } }
        public bool locked { get { return si.locked; } }
        public byte angle { get { return si.angle; } }
        public byte led { get { return si.led; } }

        private bool flashShow = false;

        DispatcherTimer flashTimer = new DispatcherTimer();

        public UcServo(data.ServoInfo si)
        {
            InitializeComponent();
            this.si = si;
            this.isSelected = false;
            InitTimer();
            SetDisplay();
        }

        private void InitTimer()
        {
            flashTimer.Tick += new EventHandler(flashTimer_Tick);
            flashTimer.Interval = TimeSpan.FromMilliseconds(300);
            flashTimer.Stop();
        }

        private void flashTimer_Tick(object sender, EventArgs e)
        {
            flashTimer.Stop();
            flashShow = !flashShow;

            if (flashShow)
            {
                eControl.StrokeThickness = (this.isSelected ? SELECTED_THICKNESS : NORMAL_THICKNESS);
            }
            else
            {
                eControl.StrokeThickness = 0;
            }
            flashTimer.Start();
        }


        private void ucMouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDownEventHandler(this, e);
        }

        public void Reset()
        {
            isSelected = false;
            isActive = false;
        }

        public void Show()
        {
            SetAngle(si.angle);
            SetDisplay();
        }

        public void SetActive(bool active)
        {
            this.isActive = (si.exists ? active : false);
            SetDisplay();
        }

        public void SetAngle(byte angle)
        {
            si.angle = angle;
            if (angle > CONST.SERVO.MAX_ANGLE)
            {
                lblAngle.Content = "?";
            }
            else
            {
                lblAngle.Content = angle;
            }
        }

        public void SetLED(byte mode)
        {
            if ((mode != CONST.LED.NO_CHANGE) && (mode != CONST.LED.TURN_ON) && (mode != CONST.LED.TURN_OFF)) return;
            si.led = mode;
        }
        /*
        public void Setexists(bool exists)
        {
            si.exists = exists;
            SetDisplay();
        }
        */
        public void SetSelection(bool selected)
        {
            this.isSelected = (si.exists ? selected : false);
            SetDisplay();
        }

        private void Servo_MouseDown(object sender, RoutedEventArgs e)
        {
            MouseDownEventHandler(this, e);
        }

        private void SetDisplay()
        {
            if (si.exists)
            {
                // Control display based on
                // 1) Locked
                // 2) Active
                eControl.Fill = (si.locked ? LOCKED_BRUSH : UNLOCK_BRUSH);
                eControl.StrokeThickness = (this.isSelected ? SELECTED_THICKNESS : NORMAL_THICKNESS);
                eControl.Stroke = (this.isSelected ? SELECTED_STROKE : NORMAL_STROKE);
                switch (this.led)
                {
                    case CONST.LED.TURN_OFF:
                        rLED.Fill = LED_OFF;
                        rLED.Visibility = Visibility.Visible;
                        break;
                    case CONST.LED.TURN_ON:
                        rLED.Fill = LED_ON;
                        rLED.Visibility = Visibility.Visible;
                        break;
                    default:
                        rLED.Visibility = Visibility.Hidden;
                        break;
                }
                if (this.isActive)
                {
                    flashTimer.Start();
                }
                else
                {
                    flashTimer.Stop();
                }

            }
            else
            {
                eControl.Fill = UNAVAILABLE_BRUSH;
                eControl.StrokeThickness = NORMAL_THICKNESS;
                eControl.Stroke = NORMAL_STROKE;
                rLED.Visibility = Visibility.Hidden;
                flashTimer.Stop();
            }
        }

    }
}
