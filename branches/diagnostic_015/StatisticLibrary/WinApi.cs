using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace StatisticCommon
{
    /// <summary>
    /// 
    /// </summary>
    static public class WinApi
    {
        /// <summary>
        /// Служит для генерации сообщения WM_SYSCOMMAND
        /// </summary>
        public static IntPtr HWND;
        /// <summary>
        /// Служит для генерации сообщения WM_SYSCOMMAND
        /// </summary>
        public const int WM_SYSCOMMAND = 0x0112;
        /// <summary>
        /// Constant value was found in the "windows.h" header file.
        /// </summary>
        public const int WM_ACTIVATEAPP = 0x001C;
        /// <summary>
        /// Для использования как wParam при WM_SYSCOMMAND. Отправляет приложению сообщение, что ПОЛЬЗОВТЕЛЬ захотел закрыть окно
        /// </summary>
        public const int SC_CLOSE = 0xF060; // close the window
        /// <summary>
        /// Для использования как wParam при WM_SYSCOMMAND. Отправляет приложению сообщение,
        /// что ПОЛЬЗОВТЕЛЬ захотел восстановить нормальный размер окна
        /// </summary>
        public const int SC_RESTORE = 0xF120;
        /// <summary>
        /// Служит для закрытия приложения от имени TaskManager
        /// </summary>
        public const int WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProcDel lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProcDel(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// Определяет свернуто ли приложение
        /// </summary>
        /// <param name="hWnd">дескриптор окна</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        public const int HWND_BROADCAST = 0xffff;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWUNTRAY = 3;
        public const int SW_RESTORE = 9;

        [DllImport("user32")]
        public static extern bool SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Найти дескриптор окна
        /// </summary>
        /// <param name="ClassName"></param>
        /// <param name="WindowName"></param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string ClassName, string WindowName);

        [DllImportAttribute("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hWnd);
    }
}
