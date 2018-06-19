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
    /// Interaction logic for UcServoMain.xaml
    /// </summary>
    public partial class UcServoMain : UserControl
    {

        public int id;
        public Point pObject;

        #region "Mouse event handler"
        public event EventHandler MouseDownEventHandler;

        private void Servo_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownEventHandler?.Invoke(this, e);
        }
        #endregion

        public UcServoMain()
        {
            InitializeComponent();
        }

        public UcServoMain(int id)
        {
            InitializeComponent();
            this.id = id;
            lblId.Content = id.ToString();
        }

    }
}