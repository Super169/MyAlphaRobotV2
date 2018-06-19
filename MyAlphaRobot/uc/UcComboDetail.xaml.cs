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
    /// Interaction logic for UcComboDetail.xaml
    /// </summary>
    public partial class UcComboDetail : UserControl
    {
        public event EventHandler SelectionChanged;

        private UTIL.DelegateUpdateInfo updateInfo;
        private data.ComboTable comboTable;
        private byte comboId;

        public UcComboDetail()
        {
            InitializeComponent();
        }

        public void InitObject(data.ComboTable comboTable, UTIL.DelegateUpdateInfo fxUpdateInfo)
        {
            updateInfo = fxUpdateInfo;
            this.comboTable = comboTable;
        }


        public void Refresh(byte comboId)
        {
            lvComboDetail.ItemsSource = null;
            this.comboId = comboId;
            if ((comboId < CONST.CI.MAX_COMBO))
            {
                lvComboDetail.ItemsSource = comboTable.combo[comboId].comboStep;
            }
            if (lvComboDetail.Items.Count > 0) lvComboDetail.SelectedIndex = 0;
        }

        private void lvComboDetail_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            data.ComboItem ci = (data.ComboItem)lvComboDetail.SelectedItem;
            if (ci != null)
            {
                SelectionChanged?.Invoke(this, new EventArgs());
            }
        }

        public data.ComboItem GetSelectedItem()
        {
            return (data.ComboItem) lvComboDetail.SelectedItem;
        }
    }
}
