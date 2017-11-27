using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System;


namespace RocketClient
{
    internal static class Win32
    {
        /// <summary>  
        /// 窗口闪动  
        /// </summary>  
        /// <param name="hwnd">窗口句柄</param>  
        /// <param name="bInvert">是否为闪</param>  
        /// <returns>成功返回0</returns>  
        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);
    }
    public class FlashWindowHelper
    {
        Timer _timer;
        int _count = 0;
        int _maxTimes = 0;
        IntPtr _window;

        public void Flash(int times, double millliseconds, IntPtr window)
        {
            _maxTimes = times;
            _window = window;

            _timer = new Timer();
            _timer.Interval = millliseconds;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (++_count < _maxTimes)
            {
                Win32.FlashWindow(_window, (_count % 2) == 0);
            }
            else
            {
                _timer.Stop();
            }
        }
    }
}