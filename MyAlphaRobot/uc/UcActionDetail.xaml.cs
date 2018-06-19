using MyAlphaRobot.data;
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
    /// Interaction logic for UcActionDetail.xaml
    /// </summary>
    public partial class UcActionDetail : UserControl
    {
        public event EventHandler DoubleClick;
        public event EventHandler EnableChanged;

        public int SelectedIndex { get { return lvActionDetail.SelectedIndex; }}
        public int PoseCount { get { return lvActionDetail.Items.Count; } }
        public bool LastSelected {  get { return (lvActionDetail.Items.Count == 0 ? false : lvActionDetail.Items.Count == lvActionDetail.SelectedIndex + 1); } }
        private int actionId = -1;

        data.ActionTable actionTable;


        public UcActionDetail()
        {
            InitializeComponent();
        }

        public void InitObject(data.ActionTable actionTable)
        {
            this.actionTable = actionTable;

            GridView gv = (GridView)lvActionDetail.View;

            for (int i = 1; i <= CONST.MAX_SERVO; i++)
            {
                gv.Columns.Add(GetServoColumn(i));
            }
            gv.Columns.Add(GetSimpleColumn("音效播放", 80, "mp3DispInfo"));
            gv.Columns.Add(GetSimpleColumn("音量", 35, "mp3DispVol"));


        }

        private GridViewColumn GetSimpleColumn(string header, double width, string dataPath )
        {
            GridViewColumn gvc= new GridViewColumn();
            gvc.Header = header;
            gvc.Width = width;
            gvc.DisplayMemberBinding = new Binding(dataPath);
            return gvc;
        }

        private GridViewColumn GetServoColumn(int idx)
        {

            GridViewColumn gvc = new GridViewColumn();
            gvc.Header = String.Format("s{0:00}", idx);
            gvc.Width = 35;

            DataTemplate dt = new DataTemplate(typeof(TextBlock));

            Binding bind = new Binding();
            bind.Path = new PropertyPath(String.Format("s{0:00}", idx));

            Binding bindColor = new Binding();
            bindColor.Path = new PropertyPath(String.Format("LED{0:00}", idx));

            FrameworkElementFactory lblElement = new FrameworkElementFactory(typeof(Label));
            lblElement.SetValue(Label.HorizontalContentAlignmentProperty, HorizontalAlignment.Right);
            lblElement.SetValue(Label.ContentProperty, bind);

            FrameworkElementFactory gridElement = new FrameworkElementFactory(typeof(Grid));
            gridElement.SetValue(Grid.MarginProperty, new Thickness(-5, 0, -5, 0));
            gridElement.SetValue(Grid.BackgroundProperty, bindColor);
            gridElement.AppendChild(lblElement);
            dt.VisualTree = gridElement;

            gvc.CellTemplate = dt;

            return gvc;
        }



        public void Refresh()
        {
            if (this.actionId == -1) return;

            int currIdx = lvActionDetail.SelectedIndex;
            Refresh(this.actionId);
            lvActionDetail.SelectedIndex = currIdx;
        }


        public void Refresh(int actionId)
        {
            lvActionDetail.ItemsSource = null;
            if ((actionId >= 0) && (actionId < CONST.AI.MAX_ACTION))
            {
                lvActionDetail.ItemsSource = actionTable.action[actionId].pose;
            }
            lvActionDetail.SelectedIndex = -1;
            this.actionId = actionId;
        }

        public data.PoseInfo GetSelectedPose()
        {
            data.PoseInfo pi = (data.PoseInfo) lvActionDetail.SelectedItem;
            return pi;

        }

        public void SetSelectedIndex(int index)
        {
            lvActionDetail.SelectedIndex = index;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as PoseInfo;
            if (item != null)
            {
                DoubleClick(this, e);
            }
        }

        private void Enable_Changed(object sender, RoutedEventArgs e)
        {
            EnableChanged?.Invoke(this, new EventArgs());
        }
    }
}
