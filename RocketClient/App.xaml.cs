using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RocketClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }
        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止，请进行重试，如果问题继续存在，请联系管理员." + "\r\nExceptionMessage:" + e.Exception.Message + "\r\nExceptionData" + e.Exception.Data + "\r\n" + "\r\nException-InnerExceptionMessage:" + e.Exception.InnerException?.Message + "\r\nStackTrace:" + e.Exception.StackTrace, "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);//这里通常需要给用户一些较为友好的提示，并且后续可能的操作

            e.Handled = true;//使用这一行代码告诉运行时，该异常被处理了，不再作为UnhandledException抛出了。
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show("我们很抱歉，当前应用程序遇到一些问题，该操作已经终止，请进行重试，如果问题继续存在，请联系管理员." + "\r\nExceptionMessage:" + e.ExceptionObject.ToString(), "意外的操作", MessageBoxButton.OK, MessageBoxImage.Information);//这里通常需要给用户一些较为友好的提示，并且后续可能的操作
        }
    }

}
