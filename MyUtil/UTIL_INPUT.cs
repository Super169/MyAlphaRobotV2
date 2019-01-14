using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyUtil
{
    public static partial class UTIL
    {
        public class INPUT
        {
            public static void PreviewCommand(ref TextCompositionEventArgs e)
            {
                e.Handled = new Regex("[^0-9A-Fa-f.]+").IsMatch(e.Text);
            }

            public static void PreviewIP(ref TextCompositionEventArgs e)
            {
                e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);
            }

            public static void PreviewInteger(ref TextCompositionEventArgs e)
            {
                e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
            }

            public static void PreviewHex(ref TextCompositionEventArgs e)
            {
                e.Handled = new Regex("[^0-9A-Fa-f]+").IsMatch(e.Text);
            }

            public static void PreviewHexMix(ref TextCompositionEventArgs e)
            {
                e.Handled = new Regex("[^0-9A-Fa-f.]+").IsMatch(e.Text);
            }

            public static void PreviewKeyDown_nospace(ref KeyEventArgs e)
            {
                e.Handled = (e.Key == Key.Space);
            }

        }
    }
}
