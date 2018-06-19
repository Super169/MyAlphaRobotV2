using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MyAlphaRobot.uc
{
    /// <summary>
    /// Interaction logic for UcComboDisplay.xaml
    /// </summary>
    public partial class UcComboDisplay : UserControl
    {
        private UTIL.DelegateUpdateInfo updateInfo;

        private data.ComboTable comboTable;

        public UcComboDisplay()
        {
            InitializeComponent();
        }

        public void InitObject(data.ComboTable comboTable, UTIL.DelegateUpdateInfo fxUpdateInfo)
        {
            updateInfo = fxUpdateInfo;
            this.comboTable = comboTable;
            ucComboList.InitObject(comboTable, fxUpdateInfo);
            ucComboDetail.InitObject(comboTable, fxUpdateInfo);
        }

        public void Refresh()
        {
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                Application.Current.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  (Action)(() => Refresh()));
                return;
            }
            ucComboList.Refresh();
        }

        private void ucComboList_SelectionChanged(object sender, EventArgs e)
        {
            updateInfo?.Invoke();
            int comboId = ucComboList.GetSelectedId();
            if ( (comboId >= 0) && (comboId < CONST.CI.MAX_COMBO))
            {
                ucComboDetail.Refresh((byte)comboId);
            }
        }

        private void ucComboDetail_SelectionChanged(object sender, EventArgs e)
        {
            updateInfo?.Invoke();
            data.ComboItem ci = ucComboDetail.GetSelectedItem();
            if (ci != null)
            {
                ucComboEdit.Refresh(ci);
            }
        }
    }
}
