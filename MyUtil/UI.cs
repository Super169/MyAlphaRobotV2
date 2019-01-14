using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MyUtil
{
    public static partial class UI
    {
        public static void UpdateTextbox(System.Windows.Controls.TextBox tb, string msg, bool async = false)
        {
            if (System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread) == null)
            {
                if (async)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateTextbox(tb, msg)));
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateTextbox(tb, msg)));
                }
                return;
            }
            tb.Text = msg;
        }

        public static void AppendTextbox(System.Windows.Controls.TextBox tb, string msg, bool async = false)
        {
            if (System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread) == null)
            {
                if (async)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => AppendTextbox(tb, msg)));
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => AppendTextbox(tb, msg)));
                }
                return;
            }
            tb.Text += msg;
        }

        public static void UpdateInfo(StatusBar sb, System.Windows.Controls.TextBlock tb, string msg, UTIL.InfoType iType = UTIL.InfoType.message, bool async = false  )
        {
            if (System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread) == null)
            {
                if (async)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateInfo(sb, tb, msg, iType, async)));
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => UpdateInfo(sb, tb, msg, iType, async)));
                }
                return;
            }
            switch (iType)
            {
                case UTIL.InfoType.message:
                    sb.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7A, 0xCC));
                    break;
                case UTIL.InfoType.alert:
                    sb.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xCA, 0x51, 0x00));
                    break;
                case UTIL.InfoType.error:
                    sb.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
                    break;

            }
            tb.Text = msg;
            return;
        }

        public static void RunInUIThread(UTIL.DelegateUIMethod fxUI, Object parm = null, bool async = false)
        {
            if (System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread) == null)
            {
                if (async)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => RunInUIThread(fxUI, parm, async)));
                }
                else
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(
                      System.Windows.Threading.DispatcherPriority.Normal,
                      (Action)(() => RunInUIThread(fxUI, parm, async)));
                }
                return;
            }
            fxUI?.Invoke(parm);
        }

        public static bool MessageConfirm(String msg)
        {
            MessageBoxResult result = MessageBox.Show(msg, "請確定", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return (result == MessageBoxResult.Yes);
        }
    }
}
