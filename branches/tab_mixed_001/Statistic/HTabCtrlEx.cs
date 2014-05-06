using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;

namespace Statistic
{
    public partial class HTabCtrlEx : System.Windows.Forms.TabControl
    {
        public delegate void DelegateOnCloseTab(object sender, CloseTabEventArgs e);
        public event DelegateOnCloseTab OnClose;

        public HTabCtrlEx()
        {
            InitializeComponent();
            this.TabStop = false;
        }

        public HTabCtrlEx(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private bool confirmOnClose = true;
        public bool ConfirmOnClose
        {
            get
            {
                return this.confirmOnClose;
            }
            set
            {
                this.confirmOnClose = value;
            }
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// override to draw the close button
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="e"></param></span>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            RectangleF tabTextArea = RectangleF.Empty;
            for (int nIndex = 0; nIndex < this.TabCount; nIndex++)
            {
                Image img;
                tabTextArea = (RectangleF)this.GetTabRect(nIndex);
                if (nIndex != this.SelectedIndex)
                {
                    img = Statistic.Properties.Resources.closeNonActive.ToBitmap();
                }
                else
                {
                    img = Statistic.Properties.Resources.closeInActive.ToBitmap();
                }

                using (img)
                {
                    e.Graphics.DrawImage(img, tabTextArea.X + tabTextArea.Width - 16, 5, 13, 13);
                }

                string str = this.TabPages[nIndex].Text;
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Near;
                using (SolidBrush brush = new SolidBrush(this.TabPages[nIndex].ForeColor))
                {
                    /*Draw the tab header text*/
                    e.Graphics.DrawString(str, this.Font, brush, tabTextArea, stringFormat);
                }
            }
        }

        /// <span class="code-SummaryComment"><summary></span>
        /// Get the stream of the embedded bitmap image
        /// <span class="code-SummaryComment"></summary></span>
        /// <span class="code-SummaryComment"><param name="filename"></param></span>
        /// <span class="code-SummaryComment"><returns></returns></span>
        private Stream GetContentFromResource(string filename)
        {
            Assembly asm = Assembly.GetExecutingAssembly ();
            Stream stream = asm.GetManifestResourceStream ("HTabCtrlLibrary." + filename);

            return stream;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {

            if (!DesignMode)
            {
                Image img;
                Graphics g = CreateGraphics();
                g.SmoothingMode = SmoothingMode.AntiAlias;
                for (int nIndex = 0; nIndex < this.TabCount; nIndex++)
                {
                    RectangleF tabTextArea = (RectangleF)this.GetTabRect(nIndex);
                    tabTextArea = new RectangleF(tabTextArea.X + tabTextArea.Width - 22, 4, tabTextArea.Height - 3, tabTextArea.Height - 5);

                    Point pt = new Point(e.X, e.Y);
                    if (tabTextArea.Contains(pt))
                    {
                        img = Statistic.Properties.Resources.closeInActive.ToBitmap();
                    }
                    else
                    {
                        if (! (nIndex == SelectedIndex))
                        {
                            img = Statistic.Properties.Resources.closeNonActive.ToBitmap();
                        }
                        else
                            img = Statistic.Properties.Resources.closeInActive.ToBitmap();
                    }

                    using (img)
                    {
                        g.DrawImage(img, tabTextArea.X + tabTextArea.Width - 12, 5, 13, 13);
                    }
                }
                g.Dispose();
            }
            else
                ;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!DesignMode)
            {
                RectangleF tabTextArea = (RectangleF)this.GetTabRect(SelectedIndex);
                tabTextArea = new RectangleF(tabTextArea.X + tabTextArea.Width - 22, 4, tabTextArea.Height - 3, tabTextArea.Height - 5);
                Point pt = new Point(e.X, e.Y);
                if (tabTextArea.Contains(pt) == true)
                {
                    if (confirmOnClose == true)
                    {
                        if (MessageBox.Show("Вы закрывайте вкладку с объектом: " + this.TabPages[SelectedIndex].Text.TrimEnd() + ". Продолжить?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.No)
                            return;
                        else
                            ;
                    }
                    else
                        ;

                    //Fire Event to Client
                    if (! (OnClose == null))
                    {
                        OnClose(this, new CloseTabEventArgs(SelectedIndex, this.TabPages[SelectedIndex].Text/*.TrimEnd()*/));
                    }
                    else
                        ;
                }
            }
        }
    }

    public class CloseTabEventArgs : EventArgs
    {
        private int nTabIndex = -1;
        private string strHeaderText = string.Empty;
        public CloseTabEventArgs(int nTabIndex, string text)
        {
            this.nTabIndex = nTabIndex;
            this.strHeaderText = text;
        }
        /// <summary>
        /// Get/Set the tab index value where the close button is clicked
        /// </summary>
        public int TabIndex
        {
            get
            {
                return this.nTabIndex;
            }
            set
            {
                this.nTabIndex = value;
            }
        }

        /// <summary>
        /// Get/Set the tab index value where the close button is clicked
        /// </summary>
        public string TabHeaderText
        {
            get
            {
                return this.strHeaderText;
            }
            set
            {
                this.strHeaderText = value;
            }
        }
    }
}
