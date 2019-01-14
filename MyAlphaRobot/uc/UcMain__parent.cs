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
    public class UcMain__parent : UserControl
    {
        protected UBTController UBT;

        protected MyUtil.UTIL.DelegateUpdateInfo updateInfo;
        protected void UpdateInfo(string msg = "", MyUtil.UTIL.InfoType iType = MyUtil.UTIL.InfoType.message, bool async = false)
        {
            updateInfo?.Invoke(msg, iType, async);
        }

        protected MyUtil.UTIL.DelegateCommandHandler commandHandler;
        protected void ParentCommand(string command, object parm)
        {
            commandHandler?.Invoke(command, parm);
        }

        public UcMain__parent()
        {

        }

        public void InitObject(UBTController UBT, 
                               MyUtil.UTIL.DelegateUpdateInfo fxUpdateInfo, 
                               MyUtil.UTIL.DelegateCommandHandler fxCommanderHandler)
        {
            this.UBT = UBT;
            this.updateInfo = fxUpdateInfo;
            this.commandHandler = fxCommanderHandler;
        }

        static protected bool MessageConfirm(String msg)
        {
            MessageBoxResult result = MessageBox.Show(msg, "請確定", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return (result == MessageBoxResult.Yes);
        }


        protected void tb_PreviewInteger(object sender, TextCompositionEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewInteger(ref e);
        }

        protected void tb_PreviewHex(object sender, TextCompositionEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewHex(ref e);
        }

        protected void tb_PreviewKeyDown_nospace(object sender, System.Windows.Input.KeyEventArgs e)
        {
            MyUtil.UTIL.INPUT.PreviewKeyDown_nospace(ref e);
        }

        static protected bool ValidInteger(TextBox tb, int min, int max, string fieldName)
        {
            int value;
            bool valid = int.TryParse(tb.Text, out value);
            if ((value >= min) && (value <= max)) return true;
            string msg = String.Format("{0} 的数值 '{3}' 不正确\n\n请输入 {1} 至 {2} 之间的数值.", fieldName, min, max, tb.Text);
            MessageBox.Show(msg, "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        static protected bool ValidString(TextBox tb, int min, int max, string fieldName)
        {
            int len = System.Text.Encoding.ASCII.GetByteCount(tb.Text);
            if ((len >= min) && (len <= max)) return true;
            string msg = String.Format("{0} 的长度为 '{3}' 不正确\n\n请输入长度在 {1} 至 {2} 之间的字串.", fieldName, min, max, tb.Text);
            MessageBox.Show(msg, "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

    }
}
