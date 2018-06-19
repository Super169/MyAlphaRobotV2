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

namespace MyAlphaRobot
{
    /// <summary>
    /// Interaction logic for UcRobotMaintenance.xaml
    /// </summary>
    public partial class UcRobotMaintenance : UserControl
    {
        public List<UcServoMain> servos = new List<UcServoMain>();
        UcServoMain dragServo = null;
        double minX, minY, maxX, maxY;

        ConfigObject config;

        public UcRobotMaintenance()
        {
            InitializeComponent();
            InitializeControls();
        }

        private void InitializeControls()
        {
            minX = 0;
            minY = 0;

            // There has no need to add handler for MouseMove
            AddHandler(Mouse.PreviewMouseUpOutsideCapturedElementEvent, new MouseButtonEventHandler(Super_MouseUp), true);
            SetStatus();
        }

        public void SetConfig(ConfigObject config)
        {
            this.config = config.Clone();
            servos = null;
            servos = new List<UcServoMain>();
            gridPanel.Children.Clear();
            LoadImage();
            for (int i = 0; i < config.max_servo; i++ )
            {
                AddServo(config.servos[i].X, config.servos[i].Y);
            }
            SetStatus();
        }

        public void LoadImage()
        {
            gridPanel.Background = config.getImageBrush();
        }

        public void ChangeImage(string fileName)
        {
            if (config.ChangeImage(fileName))
            {
                LoadImage();
            }
        }

        private void gridPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            maxX = gridPanel.ActualWidth - CONST.SERVO_SIZE;
            maxY = gridPanel.ActualHeight - CONST.SERVO_SIZE;
        }

        private void btnAddControl_Click(object sender, RoutedEventArgs e)
        {
            if (servos.Count >= CONST.MAX_ROBOT_SERVO)
            {
                // It's not logic, button should be disabled already
                btnAddControl.IsEnabled = false;
                return;
            }
            UcServoMain s = new UcServoMain(servos.Count + 1);
            s.Width = 38;
            s.Height = 38;
            s.HorizontalAlignment = HorizontalAlignment.Left;
            s.VerticalAlignment = VerticalAlignment.Top;
            s.Margin = new Thickness(0, 0, 0, 0);
            s.MouseDownEventHandler += Servo_MouseDownEventHandler;
            gridPanel.Children.Add(s);
            servos.Add(s);
            SetStatus();
        }

        private void AddServo(double x, double y)
        {
            UcServoMain s = new UcServoMain(servos.Count + 1);
            s.Width = 38;
            s.Height = 38;
            s.HorizontalAlignment = HorizontalAlignment.Left;
            s.VerticalAlignment = VerticalAlignment.Top;
            s.Margin = new Thickness(x, y, 0, 0);
            s.MouseDownEventHandler += Servo_MouseDownEventHandler;
            gridPanel.Children.Add(s);
            servos.Add(s);
        }


        private void btnRemoveControl_Click(object sender, RoutedEventArgs e)
        {
            if (servos.Count == 0)
            {
                btnRemoveControl.IsEnabled = false;
                return;
            }
            UcServoMain s = servos.ElementAt(servos.Count - 1);
            gridPanel.Children.Remove(s);
            servos.Remove(s);
            SetStatus();
        }

        private void SetStatus()
        {
            txtServoCount.Text = servos.Count.ToString();
            btnAddControl.IsEnabled = (servos.Count < CONST.MAX_ROBOT_SERVO);
            btnRemoveControl.IsEnabled = (servos.Count > 0);

        }

        private void Servo_MouseDownEventHandler(object sender, EventArgs e)
        {
            UcServoMain s = (UcServoMain)sender;
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.LeftButton == MouseButtonState.Pressed)
            {
                s.pObject = me.GetPosition(s);
                s.SetValue(Panel.ZIndexProperty, 999);
                dragServo = s;
                Mouse.Capture(gridPanel);
            }
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragServo != null)
            {
                Point pCurrent = e.GetPosition(gridPanel);
                double newX = pCurrent.X - dragServo.pObject.X;
                double newY = pCurrent.Y - dragServo.pObject.Y;
                newX = Math.Min(Math.Max(newX, minX), maxX);
                newY = Math.Min(Math.Max(newY, minY), maxY);
                dragServo.Margin = new Thickness(newX, newY, 0, 0);
            }
        }

        private void Super_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dragServo != null)
            {
                dragServo.SetValue(Panel.ZIndexProperty, 0);
                dragServo = null;
                Mouse.Capture(null);
            }
        }

        public ConfigObject GetConfigObject()
        {
            config.max_servo = servos.Count;
            config.servos = new List<Point>();
            for (int i = 0; i < servos.Count; i++)
            {
                config.servos.Add(new Point(servos[i].Margin.Left, servos[i].Margin.Top));
            }
            return config;
        }
    }
}
