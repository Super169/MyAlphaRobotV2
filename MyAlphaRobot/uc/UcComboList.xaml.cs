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
    /// Interaction logic for UcComboList.xaml
    /// </summary>
    public partial class UcComboList : UserControl
    {
        public event EventHandler SelectionChanged;

        private MyUtil.UTIL.DelegateUpdateInfo updateInfo;

        private data.ComboTable comboTable;

        public UcComboList()
        {
            InitializeComponent();
        }

        public void InitObject(data.ComboTable comboTable, MyUtil.UTIL.DelegateUpdateInfo fxUpdateInfo)
        {
            updateInfo = fxUpdateInfo;
            this.comboTable = comboTable;
            lvComboList.ItemsSource = comboTable.combo;
            
        }

        public void Refresh()
        {
            /*
            if (Dispatcher.FromThread(Thread.CurrentThread) == null)
            {
                Application.Current.Dispatcher.BeginInvoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  (Action)(() => Refresh()));
                return;
            }
            */
            int currIdx = lvComboList.SelectedIndex;
            ICollectionView view = CollectionViewSource.GetDefaultView(lvComboList.ItemsSource);
            view.Refresh();
            lvComboList.SelectedIndex = currIdx;
        }

        private void lvComboList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            data.ComboInfo ci = (data.ComboInfo)lvComboList.SelectedItem;
            if (ci != null)
            {
                SelectionChanged?.Invoke(this, new EventArgs());
            }
        }

        public int GetSelectedId()
        {
            data.ComboInfo ci = (data.ComboInfo)lvComboList.SelectedItem;
            if (ci == null) return -1;
            return ci.comboId;

        }
    }
}
