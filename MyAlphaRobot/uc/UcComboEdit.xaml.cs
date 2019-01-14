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

namespace MyAlphaRobot.uc
{
    /// <summary>
    /// Interaction logic for UcComboEdit.xaml
    /// </summary>
    public partial class UcComboEdit : UserControl
    {
        public UcComboEdit()
        {
            InitializeComponent();
        }

        private void tb_PreviewInteger(object sender, TextCompositionEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewInteger(ref e);
        }

        public void Refresh(data.ComboItem ci)
        {
            if (ci == null) return;
            tbActionId.Text = ci.actionId.ToString();
            tbRepeatCount.Text = ci.repeatCount.ToString();
        }
    }
}
