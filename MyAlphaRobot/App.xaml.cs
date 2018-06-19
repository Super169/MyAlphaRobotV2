using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MyAlphaRobot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
        public static DateTime buildDateTime = new DateTime(2000, 1, 1).Add(new TimeSpan(
                                               TimeSpan.TicksPerDay * version.Build + // days since 1 January 2000
                                               TimeSpan.TicksPerSecond * 2 * version.Revision)); // seconds since midnight, (multiply by 2 to get original) 
        public string winTitle = string.Format("MyAlphaRobot v{0}  [{1:yyyy-MM-dd HH:mm}]  (No Copyright © 2018 Super169)", version, buildDateTime);

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            MainWindow.Title = winTitle;
            MainWindow.Closing += MainWindow_Closing;
            ShowMainWindow();
        }

        public void ShowMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }

        public void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            ((MainWindow)MainWindow).OnWindowClosing(sender, e);
        }

    }
}
