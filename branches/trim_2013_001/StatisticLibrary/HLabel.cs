using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace StatisticCommon
{
    public partial class HLabel : System.Windows.Forms.Label
    {
        public HLabel(Point pt, Size sz, Color foreColor, Color backColor, Single szFont, ContentAlignment align)
        {
            InitializeComponent();

            this.BorderStyle = BorderStyle.Fixed3D;

            if (((pt.X < 0) || (pt.Y < 0)) ||
                ((sz.Width < 0) || (sz.Height < 0)))
                this.Dock = DockStyle.Fill;
            else {
                this.Location = pt;
                this.Size = sz;
            }

            this.ForeColor = foreColor;
            this.BackColor = backColor;

            this.TextAlign = align;

            //this.Text = text;
        }

        public HLabel(HLabelStyles prop) : this (new Point (-1, -1), new Size (-1, -1), prop.m_foreColor, prop.m_backColor, prop.m_szFont, prop.m_align)
        {
        }

        public HLabel(IContainer container, Point pt, Size sz, Color foreColor, Color backColor, Single szFont, ContentAlignment align) : this (pt, sz, foreColor, backColor, szFont, align)
        {
            container.Add(this);
        }

        public HLabel(IContainer container, HLabelStyles prop)
            : this(new Point (-1, -1), new Size (-1, -1), prop.m_foreColor, prop.m_backColor, prop.m_szFont, prop.m_align)
        {
            container.Add(this);
        }

        public static Label createLabel(string name, HLabelStyles prop)
        {
            Label lblRes = new Label();
            lblRes.Text = name;
            lblRes.Dock = DockStyle.Fill;
            lblRes.BorderStyle = BorderStyle.Fixed3D;
            lblRes.Font = new System.Drawing.Font("Microsoft Sans Serif", prop.m_szFont, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            lblRes.TextAlign = prop.m_align;
            lblRes.ForeColor = prop.m_foreColor;
            lblRes.BackColor = prop.m_backColor;

            return lblRes;
        }
    }

    public class HLabelStyles
    {
        public Color m_foreColor,
                    m_backColor;
        public ContentAlignment m_align;
        public Single m_szFont;

        public HLabelStyles(Color foreColor, Color backColor, Single szFont, ContentAlignment align)
        {
            m_foreColor = foreColor;
            m_backColor = backColor;
            m_szFont = szFont;
            m_align = align;
        }
    };
}
