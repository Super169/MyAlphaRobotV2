using MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyAlphaRobot
{
    public partial class MainWindow : Window
    {
        public UBTController UBT;

        private void InitUBT()
        {
            UBT = new UBTController(UpdateInfoCallback, UpdateServoCallback, robot);
        }

    }
}
