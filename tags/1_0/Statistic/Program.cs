using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Statistic
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            InitTEC init = new InitTEC();

            Application.Run(new MainForm(init.tec));
        }
    }
}