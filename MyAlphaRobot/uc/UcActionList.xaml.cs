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
    /// Interaction logic for UcActionList.xaml
    /// </summary>
    public partial class UcActionList : UserControl
    {

        private MyUtil.UTIL.DelegateUpdateInfo updateInfo;

        public event EventHandler SelectionChanged;
        public event EventHandler DoubleClick;

        public event EventHandler PlayAction;
        public event EventHandler StopAction;

        public event EventHandler InsertPose;
        public event EventHandler InsertPoseBefore;
        public event EventHandler InsertPoseAfter;
        public event EventHandler DeletePose;
        public event EventHandler CopyToEnd;

        public int SelectedIndex { get { return lvActionList.SelectedIndex; } }

        private data.ActionTable actionTable;

        public UcActionList()
        {
            InitializeComponent();
        }

        public void InitObject(data.ActionTable actionTable, MyUtil.UTIL.DelegateUpdateInfo fxUpdateInfo)
        {
            updateInfo = fxUpdateInfo;
            this.actionTable = actionTable;
            lvActionList.ItemsSource = actionTable.action;
        }

        private void UpdateInfo(string msg = "", MyUtil.UTIL.InfoType iType = MyUtil.UTIL.InfoType.message, bool async = false)
        {
            updateInfo?.Invoke(msg, iType, async);
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
            int currIdx = lvActionList.SelectedIndex;
            ICollectionView view = CollectionViewSource.GetDefaultView(lvActionList.ItemsSource);
            view.Refresh();
            lvActionList.SelectedIndex = currIdx;
            data.ActionInfo ai = (data.ActionInfo)lvActionList.SelectedItem;
            tbActionName.Text = (ai == null ? "" : ai.actionName);
        }

        private void lvActionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            data.ActionInfo ai = (data.ActionInfo)lvActionList.SelectedItem;
            if (ai == null)
            {
                tbActionName.Text = "";
                gridActionEdit.IsEnabled = false;
            } else
            {
                tbActionName.Text = ai.actionName;
                gridActionEdit.IsEnabled = true;
            }
            SelectionChanged?.Invoke(this, new EventArgs());
        }

        public int GetSelectedActionId()
        {
            data.ActionInfo ai = (data.ActionInfo)lvActionList.SelectedItem;
            if (ai == null) return -1;
            return ai.actionId;
        }

        public bool SelectIndex(int newIdx)
        {
            if (newIdx < -1) return false;
            if (newIdx >= lvActionList.Items.Count) return false;
            lvActionList.SelectedIndex = newIdx;
            return true;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as data.ActionInfo;
            if (item != null)
            {
                DoubleClick(this, e);
            }
        }

        private void btnUpdateAction_Click(object sender, RoutedEventArgs e)
        {
            UpdateInfo();
            data.ActionInfo ai = (data.ActionInfo)lvActionList.SelectedItem;
            int byteCnt = Encoding.UTF8.GetByteCount(tbActionName.Text.Trim());
            if (byteCnt > 20)
            {
                UpdateInfo(String.Format("New name has {0} bytes in UTF-8 encoding, only 20 bytes is allowed.", byteCnt), MyUtil.UTIL.InfoType.error);
                return;
            }
            ai.actionName = tbActionName.Text.Trim();
            this.Refresh();
        }


        private void btnPlayAction_Click(object sender, RoutedEventArgs e)
        {
            PlayAction?.Invoke(this, new EventArgs());
        }

        private void btnStopAction_Click(object sender, RoutedEventArgs e)
        {
            StopAction?.Invoke(this, new EventArgs());
        }

        private void btnNewPose_Click(object sender, RoutedEventArgs e)
        {
            InsertPose?.Invoke(this, new EventArgs());
        }

        private void btnInsertPoseBefore_Click(object sender, RoutedEventArgs e)
        {
            InsertPoseBefore?.Invoke(this, new EventArgs());
        }

        private void btnInsertPoseAfter_Click(object sender, RoutedEventArgs e)
        {
            InsertPoseAfter?.Invoke(this, new EventArgs());
        }

        private void btnDeletePose_Click(object sender, RoutedEventArgs e)
        {
            DeletePose?.Invoke(this, new EventArgs());
        }

        private void btnCopyToEnd_Click(object sender, RoutedEventArgs e)
        {
            CopyToEnd?.Invoke(this, new EventArgs());
        }

    }
}
